using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DynamicData.Binding;
using KubeCtrlUI.Models;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace KubeCtrlUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly ILogger<MainWindowViewModel> log;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        private string status = "Loading contexts...";
        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }
        
        #region Context Data (tab 1)
        private readonly K8SConfiguration kubeConfig;
        public ObservableCollection<KubeContext> Contexts { get; set; } = new();
        
        private KubeContext? selectedContext;
        public KubeContext? SelectedContext
        {
            get => selectedContext;
            set
            {
                if (selectedContext != value)
                {
                    selectedContext = value;
                    OnPropertyChanged();
                    // Optional: you can switch context here if you want
                    // SwitchContextCommand.Execute(value);
                }
            }
        }    
        #endregion
        
        #region namespace data (tab 2)
        public ObservableCollection<KubeNamespace> Namespaces { get; set; } = new();
        #endregion
        
        #region View interactions/commands
        public void SwitchContext(KubeContext context)
        {
            if (context == null || context.Name == kubeConfig.CurrentContext)
                return;
            try
            {
                kubeConfig.CurrentContext = context.Name;
                // KubernetesClientConfiguration.LoadKubeConfig(kubeConfig);

                Status = $"Switched to context: {context.Name}";
                LoadContexts(); // Refresh the list to update the green "*"
                SelectedContext = context;
            }
            catch (Exception ex)
            {
                Status = $"Failed to switch context: {ex.Message}";
            }
        }
        
        public void RefreshNamespaces()
        {
            try
            {
                LoadNamespaces();
            }
            catch (Exception ex)
            {
                Status = $"Failed to load namespaces: {ex.Message}";
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            try
            {
                log = App.CreateLogger<MainWindowViewModel>();
                kubeConfig = KubernetesClientConfiguration.LoadKubeConfig();
                LoadContexts();
            }
            catch (Exception ex)
            {
                Status = $"Failed to load kubeconfig: {ex.Message}";
            }
        }
        
        private void LoadContexts()
        {
            if (kubeConfig == null)
            {
                Status = "No kubeconfig loaded.";
                return;
            }

            Contexts.Clear();

            foreach (var ctx in kubeConfig.Contexts)
            {
                var isCurrent = ctx.Name == kubeConfig.CurrentContext;

                var kubeContext = new KubeContext(
                    Name: ctx.Name,
                    Cluster: ctx.ContextDetails.Cluster,
                    AuthInfo: ctx.ContextDetails.User,
                    Namespace: ctx.ContextDetails.Namespace ?? "<default>",
                    IsCurrent: isCurrent);

                Contexts.Add(kubeContext);

                if (isCurrent)
                    SelectedContext = kubeContext;
            }

            Status = $"Loaded {Contexts.Count} context(s). Current: {kubeConfig.CurrentContext}";
        }
        
        private void LoadNamespaces()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            config.Namespace = SelectedContext?.Namespace;
            using var client = new Kubernetes(config);

            List<string> data = GetNamespacesInternalAsync(client)
                .GetAwaiter()
                .GetResult();
            
            Namespaces.Clear();
            foreach (var ns in data)
            {
                var record = new KubeNamespace(
                    Name: ns,
                    IsCurrent: ns == kubeConfig.Contexts
                        .FirstOrDefault(c => c.Name == kubeConfig.CurrentContext)?
                        .ContextDetails.Namespace);
                Namespaces.Add(record);
            }
            
            Status = $"Loaded {Namespaces.Count} namespaces(s) for: {kubeConfig.CurrentContext}";
        }
        
        private static async Task<List<string>> GetNamespacesInternalAsync(IKubernetes client)
        {
            // ListNamespaceAsync calls the API endpoint: /api/v1/namespaces
            V1NamespaceList namespaceList = await client.CoreV1.ListNamespaceAsync();

            // Extract and return the namespace names
            return namespaceList.Items
                .Select(ns => ns.Metadata.Name)
                .ToList();
        }
    }
}
