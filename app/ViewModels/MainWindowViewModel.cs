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

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(); }
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

        private KubeNamespace? selectedNamespace;
        public KubeNamespace? SelectedNamespace
        {
            get => selectedNamespace;
            set { selectedNamespace = value; OnPropertyChanged(); }
        }
        #endregion

        #region pod data (tab 3)
        public ObservableCollection<KubePod> Pods { get; set; } = new();
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
        
        public async Task RefreshNamespacesAsync()
        {
            try
            {
                IsBusy = true;
                await LoadNamespacesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Status = $"Failed to load namespaces: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void SelectNamespace(KubeNamespace ns)
        {
            SelectedNamespace = ns;
            Status = $"Selected namespace: {ns.Name}";
        }

        public async Task RefreshPodsAsync()
        {
            try
            {
                IsBusy = true;
                await LoadPodsAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Status = $"Failed to load pods: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<string> GetPodLogsAsync(string podName, string namespaceName)
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            using var client = new Kubernetes(config);

            V1Pod pod = await client.CoreV1.ReadNamespacedPodAsync(podName, namespaceName).ConfigureAwait(false);
            var containerNames = pod.Spec.Containers.Select(c => c.Name).ToList();
            var container = containerNames.First();

            using var stream = await client.CoreV1.ReadNamespacedPodLogAsync(podName, namespaceName, container: container).ConfigureAwait(false);
            using var reader = new System.IO.StreamReader(stream);
            var logText = await reader.ReadToEndAsync().ConfigureAwait(false);

            return containerNames.Count > 1
                ? $"[Showing logs for container: {container}]\n\n{logText}"
                : logText;
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
        
        private async Task LoadNamespacesAsync()
        {
            Status = "Loading namespaces...";

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            // var config = KubernetesClientConfiguration.InClusterConfig();
            // config.Namespace = SelectedContext?.Namespace;
            using var client = new Kubernetes(config);

            List<string> data = await GetNamespacesInternalAsync(client).ConfigureAwait(true);

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
            V1NamespaceList namespaceList = await client.CoreV1.ListNamespaceAsync().ConfigureAwait(false);
            // Extract and return the namespace names
            return namespaceList.Items
                .Select(ns => ns.Metadata.Name)
                .ToList();
        }

        private async Task LoadPodsAsync()
        {
            var namespaceName = SelectedNamespace?.Name
                ?? kubeConfig.Contexts.FirstOrDefault(c => c.Name == kubeConfig.CurrentContext)?.ContextDetails.Namespace;

            if (string.IsNullOrEmpty(namespaceName))
            {
                Status = "No namespace selected. Double-click a namespace on the Namespaces tab first.";
                return;
            }

            Status = $"Loading pods for: {namespaceName}...";

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            using var client = new Kubernetes(config);

            V1PodList podList = await client.CoreV1.ListNamespacedPodAsync(namespaceName).ConfigureAwait(true);

            Pods.Clear();
            foreach (var pod in podList.Items)
            {
                var containerStatuses = pod.Status.ContainerStatuses ?? new List<V1ContainerStatus>();
                var readyCount = containerStatuses.Count(cs => cs.Ready);
                var restartCount = containerStatuses.Sum(cs => cs.RestartCount);
                var age = pod.Metadata.CreationTimestamp.HasValue
                    ? FormatAge(DateTime.UtcNow - pod.Metadata.CreationTimestamp.Value.ToUniversalTime())
                    : "unknown";

                Pods.Add(new KubePod(
                    Name: pod.Metadata.Name,
                    Phase: pod.Status.Phase,
                    Ready: $"{readyCount}/{containerStatuses.Count}",
                    RestartCount: restartCount,
                    Age: age));
            }

            Status = $"Loaded {Pods.Count} pod(s) for: {namespaceName}";
        }

        private static string FormatAge(TimeSpan age)
        {
            if (age.TotalDays >= 1)
                return $"{(int)age.TotalDays}d";
            if (age.TotalHours >= 1)
                return $"{(int)age.TotalHours}h";
            return $"{(int)Math.Max(age.TotalMinutes, 0)}m";
        }
    }
}
