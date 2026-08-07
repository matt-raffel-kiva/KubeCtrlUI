using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace KubeCtrlUI.ViewModels
{
    public class PodLogsViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly Func<string, string, Task<string>> getLogsAsync;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string PodName { get; }
        public string Namespace { get; }

        public string Title => $"Logs: {PodName} ({Namespace})";

        private string logText = string.Empty;
        public string LogText
        {
            get => logText;
            set { logText = value; OnPropertyChanged(); }
        }

        private string status = string.Empty;
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

        public PodLogsViewModel(string podName, string namespaceName, Func<string, string, Task<string>> getLogsAsync)
        {
            PodName = podName;
            Namespace = namespaceName;
            this.getLogsAsync = getLogsAsync;
        }

        public async Task RefreshAsync()
        {
            try
            {
                IsBusy = true;
                Status = "Loading logs...";
                LogText = await getLogsAsync(PodName, Namespace).ConfigureAwait(true);
                Status = $"Loaded logs for {PodName}";
            }
            catch (Exception ex)
            {
                Status = $"Failed to load logs: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
