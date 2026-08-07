using Avalonia.Controls;
using Avalonia.Interactivity;
using KubeCtrlUI.ViewModels;

namespace KubeCtrlUI.Views
{
    public partial class PodLogsWindow : Window
    {
        public PodLogsWindow()
        {
            InitializeComponent();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PodLogsViewModel viewModel)
                await viewModel.RefreshAsync();
        }
    }
}
