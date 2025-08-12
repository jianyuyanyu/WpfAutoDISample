using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WpfAutoDISample.Common;

// ReSharper disable AsyncVoidMethod

namespace WpfAutoDISample;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly ILogger<App> _logger;

    /// <summary>
    /// 获取当前AppHost
    /// </summary>
    private static IHost CurrentAppHost => (Current as App)?.Host ?? throw new InvalidOperationException("无法获取AppHost，当前Application实例不是App类型。");

    private IHost Host { get; }

    /// <summary>
    /// Services
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IServiceProvider Services => CurrentAppHost.Services;

    /// <inheritdoc />
    public App(ref IHost host)
    {
        InitializeComponent();
        Host = host;
        _logger = Services.GetRequiredService<ILogger<App>>();
    }

    /// <inheritdoc />
    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainUnhandledException;
            DispatcherUnhandledException -= AppDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException -= TaskSchedulerOnUnobservedTaskException;
            await Host.StopAsync().ConfigureAwait(false);
            Host.Dispose();
            // 这里不要忘记释放
            WinApis.Mutex?.ReleaseMutex();
            Shutdown();
            base.OnExit(e);
        }
        catch (Exception ex)
        {
            // 处理退出时的异常
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Application exit failed.");
            }
        }
    }

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            DispatcherUnhandledException += AppDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
#endif
            await Host.StartAsync().ConfigureAwait(false);
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            // 处理启动时的异常
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(ex, "Application startup failed.");
            }
        }
    }

    private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        HandleException((Exception)e.ExceptionObject);
    }

    private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception);
        e.SetObserved();
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleException(e.Exception);
        e.Handled = true;
    }

    private void HandleException(Exception? exception)
    {
        if (exception is null)
        {
            return;
        }
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
    }
}