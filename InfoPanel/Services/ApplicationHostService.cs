using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using InfoPanel.Views.Windows;
using Microsoft.Extensions.Hosting;
using Serilog;
using Wpf.Ui;

namespace InfoPanel.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService : IHostedService
{
    private static readonly ILogger Logger = Log.ForContext<ApplicationHostService>();
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;

    private INavigationWindow? _navigationWindow;

    public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
        IPageService pageService)
    {
        // If you want, you can do something with these services at the beginning of loading the application.
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _pageService = pageService;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.Information("StartAsync: Application host service starting");
        PrepareNavigation();
        Logger.Information("StartAsync: Navigation prepared, calling HandleActivationAsync");

        await HandleActivationAsync();

        Logger.Information("StartAsync: Application host service started successfully");
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates main window during activation.
    /// </summary>
    private async Task HandleActivationAsync()
    {
        await Task.CompletedTask;

        try
        {
            Logger.Information("HandleActivationAsync: Checking for existing MainWindow");
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                Logger.Information("HandleActivationAsync: No existing MainWindow found, creating new one");

                Logger.Debug("HandleActivationAsync: Requesting INavigationWindow from service provider");
                _navigationWindow = _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;

                if (_navigationWindow == null)
                {
                    Logger.Error("HandleActivationAsync: Failed to get INavigationWindow from service provider");
                    throw new InvalidOperationException("Failed to resolve INavigationWindow from service provider");
                }

                Logger.Information("HandleActivationAsync: MainWindow created successfully, calling ShowWindow()");
                _navigationWindow.ShowWindow();
                Logger.Information("HandleActivationAsync: ShowWindow() completed");
            }
            else
            {
                Logger.Information("HandleActivationAsync: MainWindow already exists");
            }
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "HandleActivationAsync: Critical error during window activation");
            throw;
        }

        await Task.CompletedTask;
    }

    private void PrepareNavigation()
    {
        _navigationService.SetPageService(_pageService);
    }
}
