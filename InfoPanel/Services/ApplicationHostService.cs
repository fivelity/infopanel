using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using InfoPanel.Views.Windows;
using Microsoft.Extensions.Hosting;
using Serilog;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace InfoPanel.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService : IHostedService
{
    private static readonly ILogger Logger = Log.ForContext<ApplicationHostService>();
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly INavigationViewPageProvider _pageProvider;
    private readonly ThemeProvider _themeProvider;
    private readonly LayoutProvider _layoutProvider;
    private readonly WorkspaceManager _workspaceManager;

    private INavigationWindow? _navigationWindow;

    public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService,
        INavigationViewPageProvider pageProvider, ThemeProvider themeProvider, LayoutProvider layoutProvider, WorkspaceManager workspaceManager)
    {
        // If you want, you can do something with these services at the beginning of loading the application.
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _pageProvider = pageProvider;
        _themeProvider = themeProvider;
        _layoutProvider = layoutProvider;
        _workspaceManager = workspaceManager;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger.Information("StartAsync: Application host service starting");

            // Initialize theme and layout providers
            Logger.Information("StartAsync: Initializing providers...");
            await _themeProvider.LoadThemesAsync();
            Logger.Information("StartAsync: LoadThemesAsync completed, loaded {Count} themes", _themeProvider.Themes.Count);

            await _layoutProvider.LoadLayoutsAsync();
            Logger.Information("StartAsync: LoadLayoutsAsync completed, loaded {Count} layouts", _layoutProvider.Layouts.Count);

            await _workspaceManager.LoadWorkspacesAsync();
            Logger.Information("StartAsync: Provider initialization complete");

            PrepareNavigation();
            Logger.Information("StartAsync: Navigation prepared, calling HandleActivationAsync");

            await HandleActivationAsync();

            Logger.Information("StartAsync: Application host service started successfully");
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "StartAsync: Critical error during application startup");
            throw;
        }
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
        try
        {
            Logger.Information("HandleActivationAsync: Starting window activation");

            // Check if we're already on the UI thread
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Logger.Debug("HandleActivationAsync: Already on UI thread, creating window directly");
                CreateMainWindow();
            }
            else
            {
                Logger.Debug("HandleActivationAsync: Not on UI thread, dispatching to UI thread");
                await Application.Current.Dispatcher.InvokeAsync(CreateMainWindow);
            }

            Logger.Information("HandleActivationAsync: Window activation completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "HandleActivationAsync: Critical error during window activation");
            throw;
        }
    }

    private void CreateMainWindow()
    {
        try
        {
            Logger.Debug("CreateMainWindow: Checking for existing MainWindow");
            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                Logger.Information("CreateMainWindow: No existing MainWindow found, creating new one");

                Logger.Debug("CreateMainWindow: Requesting INavigationWindow from service provider");
                _navigationWindow = _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;

                if (_navigationWindow == null)
                {
                    Logger.Error("CreateMainWindow: Failed to resolve INavigationWindow from service provider");
                    throw new InvalidOperationException("Failed to resolve INavigationWindow from service provider");
                }

                Logger.Information("CreateMainWindow: MainWindow instance created, calling ShowWindow()");
                _navigationWindow.ShowWindow();
                Logger.Information("CreateMainWindow: ShowWindow() completed");
            }
            else
            {
                Logger.Information("CreateMainWindow: MainWindow already exists");
            }
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "CreateMainWindow: Critical error during window creation");
            throw;
        }
    }

    private void PrepareNavigation()
    {
        // Page provider is set directly on NavigationView in MainWindow constructor
        // No additional setup needed here for WPF-UI 4.x
    }
}
