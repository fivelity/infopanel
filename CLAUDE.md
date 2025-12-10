# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore packages
dotnet restore

# Build Debug
dotnet build InfoPanel.sln -c Debug

# Build Release
dotnet build InfoPanel.sln -c Release

# Publish for deployment (Windows x64)
dotnet publish InfoPanel/InfoPanel.csproj -c Release -r win-x64 --self-contained -p:PublishProfile=win-x64

# Run the main application
dotnet run --project InfoPanel/InfoPanel.csproj

# Run plugin simulator for testing plugins
dotnet run --project InfoPanel.Plugins.Simulator/InfoPanel.Plugins.Simulator.csproj
```

## Architecture Overview

InfoPanel is a WinUI 3 desktop application built on .NET 8.0 that displays hardware monitoring data on desktop overlays and USB LCD panels. The codebase follows MVVM architecture with a modular plugin system.

### Core Projects

- **InfoPanel** - Main WinUI 3 application
  - Entry: `App.xaml.cs`
  - MVVM structure: ViewModels handle logic, Views handle UI
  - Background services run display updates and hardware communication
  - Drawing abstraction supports SkiaSharp rendering

- **InfoPanel.Plugins** - Plugin interface definitions
  - `IPlugin` - Base plugin interface
  - `IPluginSensor` - For sensor data providers
  - `IPluginText/Table` - For display elements
  - All plugins must inherit from `BasePlugin`

- **InfoPanel.Plugins.Loader** - Dynamic plugin loading
  - Discovers plugins in the `plugins` directory
  - Loads assemblies in isolated contexts
  - Manages plugin lifecycle and dependencies

- **InfoPanel.Extras** - Built-in plugins
  - Ships with the application in the plugins folder
  - Provides system info, network, drives, weather functionality

### Key Services and Background Tasks

Located in `InfoPanel/Services/`:
- `PanelDrawTask` - Renders visualizations at high frame rates
- `BeadaPanelTask/TuringPanelTask` - USB panel communication
- `WebServerTask` - HTTP API and web interface
- Hardware monitors - Collect sensor data from HWiNFO/LibreHardwareMonitor

### Display System

The drawing system (`InfoPanel/Drawing/`) uses SkiaSharp for rendering:
- `SkiaGraphics` - Primary renderer using SkiaSharp.Views.WinUI
- `PanelDraw` - Orchestrates rendering of display items

Display items (`InfoPanel/Models/`) represent visualizations:
- `SensorDisplayItem` - Text-based sensor values
- `GaugeDisplayItem` - Circular gauge visualizations
- `ChartDisplayItem` - Graphs, bars, and donut charts
- `ImageDisplayItem` - Static and animated images

### USB Panel Support

USB panel communication is in `InfoPanel/TuringPanel/` and `InfoPanel/BeadaPanel/`:
- Uses WinUSB API for BeadaPanel devices
- Serial/USB communication for TuringPanel devices
- Model-specific configurations in database classes

## Key Technologies

- **WinUI 3 (Windows App SDK 1.6+)** for modern Windows desktop UI
- **CommunityToolkit.MVVM** for MVVM implementation
- **SkiaSharp (SkiaSharp.Views.WinUI)** for cross-platform graphics rendering
- **Serilog** for structured logging
- **LibreHardwareMonitor** for hardware sensor access
- **ASP.NET Core** for built-in web server
- **Sentry** for error tracking

## WinUI 3 Migration Guidelines

InfoPanel is in the process of being fully migrated from WPF to WinUI 3.

### UI Framework
- Use `Microsoft.UI.Xaml` namespaces instead of `System.Windows`.
- `Window` replaces `System.Windows.Window`.
- `Page` replaces `System.Windows.Controls.Page`.
- Use `DispatcherQueue` instead of `Dispatcher`.
- `ContentDialog` replaces `MessageBox` or custom dialog windows.

### Graphics (SkiaSharp)
- Use **SkiaSharp.Views.WinUI** package.
- **NEVER** use `SkiaSharp.Views.Windows` or `SkiaSharp.Views.WPF`.
- XAML Control: `<skiasharp:SKXamlCanvas x:Name="skElement" PaintSurface="OnPaintSurface"/>`
- Namespace: `xmlns:skiasharp="using:SkiaSharp.Views.WinUI"`
- Event Handler: `private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)`

### Common Migration Patterns
- **Visibility**: WinUI uses `Visibility.Visible` and `Visibility.Collapsed` (no `Hidden`).
- **Commands**: `ICommand` is still used, compatible with CommunityToolkit.Mvvm.
- **Resources**: `Application.Current.Resources` access may need thread safety checks.
- **Threading**: Use `DispatcherQueue.TryEnqueue(() => { ... })` for UI updates from background threads.

## Development Notes

- The solution uses .NET 8.0 with Windows Desktop runtime (windows10.0.19041.0+)
- Warning level 6 and nullable reference types are enabled
- Plugins are loaded from the `plugins` directory at runtime
- Configuration is stored in `%APPDATA%/InfoPanel/`
