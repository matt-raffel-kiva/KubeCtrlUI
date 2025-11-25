using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KubeCtrlUI.ViewModels;
using KubeCtrlUI.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug; 

namespace KubeCtrlUI
{
    public partial class App : Application
    {
        public static ILoggerFactory LoggerFactory { get; private set; } = null!;
        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            LoggerFactory =  Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                builder.AddDebug().SetMinimumLevel(LogLevel.Trace)
                );

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
