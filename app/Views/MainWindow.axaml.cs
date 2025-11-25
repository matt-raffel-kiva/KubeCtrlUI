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
        
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                TabItem selectedTab = tabControl.SelectedItem as TabItem;
                switch (selectedTab.Name)
                {
                    case "NamespacesTab":
                        log.LogInformation("Selected tab changed to: {Tab}", selectedTab);
                        if (DataContext is MainWindowViewModel viewModel)
                            viewModel.RefreshNamespaces();
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
            
        }
    }
}

