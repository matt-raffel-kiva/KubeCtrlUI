using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KubeCtrlUI.Models;
using KubeCtrlUI.ViewModels;
using Microsoft.Extensions.Logging;

namespace KubeCtrlUI.Views
{
    public partial class MainWindow : Window
    {
        private ILogger<MainWindow> log;

        public MainWindow()
        {
            log = App.CreateLogger<MainWindow>();
            InitializeComponent();
        }

        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // SelectionChanged bubbles up from the ListBoxes inside each tab (they're
            // Selectors too), so ignore anything that didn't originate from the TabControl
            // itself - otherwise clicking a list item looks like a tab change and reloads it.
            if (!ReferenceEquals(e.Source, sender))
                return;

            if (sender is TabControl tabControl)
            {
                TabItem selectedTab = tabControl.SelectedItem as TabItem;
                switch (selectedTab.Name)
                {
                    case "NamespacesTab":
                        log.LogInformation("Selected tab changed to: {Tab}", selectedTab);
                        if (DataContext is MainWindowViewModel viewModel)
                            await viewModel.RefreshNamespacesAsync();
                        break;
                    case "PodsTab":
                        log.LogInformation("Selected tab changed to: {Tab}", selectedTab);
                        if (DataContext is MainWindowViewModel podsViewModel)
                            await podsViewModel.RefreshPodsAsync();
                        break;
                }
            }
        }

        private void Contexts_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is ListBox listBox)
                if (listBox.SelectedItem is KubeContext context)
                    if (DataContext is MainWindowViewModel viewModel)
                        viewModel.SwitchContext(context);
        }

        private void NamespaceListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is ListBox listBox)
                if (listBox.SelectedItem is KubeNamespace ns)
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.SelectNamespace(ns);
                        MainTabControl.SelectedItem = PodsTab;
                    }
        }

        private async void PodListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is ListBox listBox)
                if (listBox.SelectedItem is KubePod pod)
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        var namespaceName = viewModel.SelectedNamespace?.Name;
                        if (string.IsNullOrEmpty(namespaceName))
                            return;

                        var logsViewModel = new PodLogsViewModel(pod.Name, namespaceName, viewModel.GetPodLogsAsync);
                        var logsWindow = new PodLogsWindow { DataContext = logsViewModel };
                        logsWindow.Show(this);
                        await logsViewModel.RefreshAsync();
                    }
        }
    }
}

