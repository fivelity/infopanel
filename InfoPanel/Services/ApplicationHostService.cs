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
            // Initialize theme and layout providers
            Log.Information("ApplicationHostService: Initializing providers...");
            await _themeProvider.LoadThemesAsync();
            Log.Information("ApplicationHostService: LoadThemesAsync completed");
            var themeCount = _themeProvider.Themes.Count;
            Log.Information("ApplicationHostService: Loaded {Count} themes", themeCount);
            
            await _layoutProvider.LoadLayoutsAsync();
            Log.Information("ApplicationHostService: Loaded {Count} layouts", _layoutProvider.Layouts.Count);
            
            await _workspaceManager.LoadWorkspacesAsync();
            Log.Information("ApplicationHostService: Provider initialization complete");

            PrepareNavigation();
            Log.Information("ApplicationHostService: Navigation prepared, calling HandleActivationAsync");

            await HandleActivationAsync();
            Log.Information("ApplicationHostService: HandleActivationAsync completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ApplicationHostService: Error during StartAsync");
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
            Log.Information("ApplicationHostService: HandleActivationAsync starting");
            
            // Must create window on UI thread
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!Application.Current.Windows.OfType<MainWindow>().Any())
                {
                    Log.Information("ApplicationHostService: Creating MainWindow on UI thread");
                    _navigationWindow = _serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow;
                    
                    if (_navigationWindow == null)
                    {
                        Log.Error("ApplicationHostService: Failed to resolve INavigationWindow from service provider");
                        return;
                    }
                    
                    Log.Information("ApplicationHostService: MainWindow created, showing window");
                    _navigationWindow.ShowWindow();
                    Log.Information("ApplicationHostService: MainWindow shown");
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ApplicationHostService: Error during HandleActivationAsync");
            throw;
        }
    }

    private void PrepareNavigation()
    {
        // Page provider is set directly on NavigationView in MainWindow constructor
        // No additional setup needed here for WPF-UI 4.x
    }
}
