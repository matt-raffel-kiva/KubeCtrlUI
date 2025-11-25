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

        private void ListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (sender is ListBox listBox)
                if (listBox.SelectedItem is KubeContext context)
                    if (DataContext is MainWindowViewModel viewModel)
                        viewModel.SwitchContext(context);
        }
    }
}

