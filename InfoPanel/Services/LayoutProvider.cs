using InfoPanel.Models;
using InfoPanel.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace InfoPanel.Services
{
    /// <summary>
    /// Service for managing layouts. Loads layout JSON files, exposes regions and placement rules,
    /// and maintains panel placement mappings.
    /// </summary>
    public class LayoutProvider
    {
        private static LayoutProvider? _instance;
        public static LayoutProvider Instance => _instance ??= new LayoutProvider();

        private readonly Dictionary<string, LayoutModel> _layouts = new();
        private string _currentLayoutId = "grid-standard";
        private readonly string _layoutsDirectory;
        private readonly Dictionary<string, PanelPlacement> _panelPlacements = new();

        public event EventHandler<LayoutChangedEventArgs>? LayoutChanged;
        public event EventHandler<PanelPlacementChangedEventArgs>? PanelPlacementChanged;

        public LayoutProvider()
        {
            // Get layouts directory from AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _layoutsDirectory = Path.Combine(appDataPath, "InfoPanel", "Layouts");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_layoutsDirectory))
            {
                Directory.CreateDirectory(_layoutsDirectory);
            }
        }

        /// <summary>
        /// Current layout ID
        /// </summary>
        public string CurrentLayoutId
        {
            get => _currentLayoutId;
            set
            {
                if (_currentLayoutId != value)
                {
                    var oldLayoutId = _currentLayoutId;
                    _currentLayoutId = value;
                    LayoutChanged?.Invoke(this, new LayoutChangedEventArgs(oldLayoutId, value, CurrentLayout));
                }
            }
        }

        /// <summary>
        /// Get the current layout
        /// </summary>
        public LayoutModel? CurrentLayout => _layouts.TryGetValue(_currentLayoutId, out var layout) ? layout : null;

        /// <summary>
        /// Get all available layouts
        /// </summary>
        public IReadOnlyDictionary<string, LayoutModel> Layouts => _layouts;

        /// <summary>
        /// Get current panel placements
        /// </summary>
        public IReadOnlyDictionary<string, PanelPlacement> PanelPlacements => _panelPlacements;

        /// <summary>
        /// Load all layouts from the layouts directory
        /// </summary>
        public async Task LoadLayoutsAsync()
        {
            _layouts.Clear();

            // Load built-in default layouts
            LoadDefaultLayouts();
            Log.Information("LayoutProvider: Loaded {Count} built-in layouts", _layouts.Count);

            // Load layouts from application directory (bundled layouts)
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var appLayoutsDirectory = Path.Combine(appDirectory, "Layouts");
            Log.Information("LayoutProvider: Checking for layouts in {Directory}", appLayoutsDirectory);
            
            if (Directory.Exists(appLayoutsDirectory))
            {
                var layoutFiles = Directory.GetFiles(appLayoutsDirectory, "*.json", SearchOption.AllDirectories);
                Log.Information("LayoutProvider: Found {Count} layout files", layoutFiles.Length);
                
                foreach (var file in layoutFiles)
                {
                    try
                    {
                        Log.Debug("LayoutProvider: Loading layout from {File}", file);
                        var json = await File.ReadAllTextAsync(file);
                        var layout = JsonSerializer.Deserialize<LayoutModel>(json);
                        if (layout != null && !string.IsNullOrEmpty(layout.Id))
                        {
                            _layouts[layout.Id] = layout;
                            Log.Information("LayoutProvider: Successfully loaded layout '{LayoutId}' from {File}", layout.Id, Path.GetFileName(file));
                        }
                        else
                        {
                            Log.Warning("LayoutProvider: Layout from {File} is null or has no ID", file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "LayoutProvider: Error loading layout from {File}", file);
                    }
                }
            }
            else
            {
                Log.Warning("LayoutProvider: Layouts directory does not exist: {Directory}", appLayoutsDirectory);
            }

            // Load custom layouts from AppData directory
            if (Directory.Exists(_layoutsDirectory))
            {
                var layoutFiles = Directory.GetFiles(_layoutsDirectory, "*.json", SearchOption.AllDirectories);
                foreach (var file in layoutFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var layout = JsonSerializer.Deserialize<LayoutModel>(json);
                        if (layout != null && !string.IsNullOrEmpty(layout.Id))
                        {
                            _layouts[layout.Id] = layout;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading layout from {file}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Load built-in default layouts
        /// </summary>
        private void LoadDefaultLayouts()
        {
            // Standard grid layout (3x3)
            _layouts["grid-standard"] = new LayoutModel
            {
                Id = "grid-standard",
                Name = "Standard Grid",
                Description = "3x3 grid layout with equal-sized regions",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Type = LayoutType.Grid,
                Grid = new GridDefinition
                {
                    Rows = 3,
                    Columns = 3,
                    RowDefinitions = new List<string> { "*", "*", "*" },
                    ColumnDefinitions = new List<string> { "*", "*", "*" },
                    Gap = 8,
                    Padding = 16
                },
                Regions = new List<LayoutRegion>
                {
                    new() { Id = "region-0-0", Name = "Top Left", Row = 0, Column = 0 },
                    new() { Id = "region-0-1", Name = "Top Center", Row = 0, Column = 1 },
                    new() { Id = "region-0-2", Name = "Top Right", Row = 0, Column = 2 },
                    new() { Id = "region-1-0", Name = "Middle Left", Row = 1, Column = 0 },
                    new() { Id = "region-1-1", Name = "Center", Row = 1, Column = 1 },
                    new() { Id = "region-1-2", Name = "Middle Right", Row = 1, Column = 2 },
                    new() { Id = "region-2-0", Name = "Bottom Left", Row = 2, Column = 0 },
                    new() { Id = "region-2-1", Name = "Bottom Center", Row = 2, Column = 1 },
                    new() { Id = "region-2-2", Name = "Bottom Right", Row = 2, Column = 2 }
                }
            };

            // Simple 2-column layout
            _layouts["grid-two-column"] = new LayoutModel
            {
                Id = "grid-two-column",
                Name = "Two Column",
                Description = "2-column layout with sidebar and main area",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Type = LayoutType.Grid,
                Grid = new GridDefinition
                {
                    Rows = 1,
                    Columns = 2,
                    RowDefinitions = new List<string> { "*" },
                    ColumnDefinitions = new List<string> { "1*", "2*" },
                    Gap = 16,
                    Padding = 16
                },
                Regions = new List<LayoutRegion>
                {
                    new() { Id = "sidebar", Name = "Sidebar", Row = 0, Column = 0 },
                    new() { Id = "main", Name = "Main Area", Row = 0, Column = 1 }
                }
            };

            // Free-form canvas layout
            _layouts["canvas-free"] = new LayoutModel
            {
                Id = "canvas-free",
                Name = "Free Canvas",
                Description = "Free-form canvas layout with drag-and-drop positioning",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Type = LayoutType.Canvas,
                Canvas = new CanvasDefinition
                {
                    Width = 1920,
                    Height = 1080,
                    AllowOverlap = true,
                    SnapToGrid = true,
                    GridSize = 8
                },
                Regions = new List<LayoutRegion>
                {
                    new()
                    {
                        Id = "canvas-main",
                        Name = "Main Canvas",
                        X = 0,
                        Y = 0,
                        Width = 1920,
                        Height = 1080,
                        AllowMultiplePanels = true
                    }
                }
            };
        }

        /// <summary>
        /// Set panel placement
        /// </summary>
        public void SetPanelPlacement(string panelId, PanelPlacement placement)
        {
            _panelPlacements[panelId] = placement;
            PanelPlacementChanged?.Invoke(this, new PanelPlacementChangedEventArgs(panelId, placement));
        }

        /// <summary>
        /// Get panel placement by ID
        /// </summary>
        public PanelPlacement? GetPanelPlacement(string panelId)
        {
            return _panelPlacements.TryGetValue(panelId, out var placement) ? placement : null;
        }

        /// <summary>
        /// Remove panel placement
        /// </summary>
        public void RemovePanelPlacement(string panelId)
        {
            _panelPlacements.Remove(panelId);
        }

        /// <summary>
        /// Clear all panel placements
        /// </summary>
        public void ClearPanelPlacements()
        {
            _panelPlacements.Clear();
        }

        /// <summary>
        /// Load panel placements from a list
        /// </summary>
        public void LoadPanelPlacements(List<PanelPlacement> placements)
        {
            _panelPlacements.Clear();
            foreach (var placement in placements)
            {
                _panelPlacements[placement.PanelId] = placement;
            }
        }

        /// <summary>
        /// Save a layout to a JSON file
        /// </summary>
        public async Task SaveLayoutAsync(LayoutModel layout)
        {
            var filePath = Path.Combine(_layoutsDirectory, $"{layout.Id}.json");
            var json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            _layouts[layout.Id] = layout;
        }

        /// <summary>
        /// Export a layout to a file
        /// </summary>
        public async Task ExportLayoutAsync(string layoutId, string filePath)
        {
            if (_layouts.TryGetValue(layoutId, out var layout))
            {
                var json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        /// <summary>
        /// Import a layout from a file
        /// </summary>
        public async Task<LayoutModel?> ImportLayoutAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var layout = JsonSerializer.Deserialize<LayoutModel>(json);
                if (layout != null && !string.IsNullOrEmpty(layout.Id))
                {
                    await SaveLayoutAsync(layout);
                    return layout;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing layout: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Get region by ID
        /// </summary>
        public LayoutRegion? GetRegion(string regionId)
        {
            return CurrentLayout?.Regions.FirstOrDefault(r => r.Id == regionId);
        }
    }

    /// <summary>
    /// Event args for layout changed event
    /// </summary>
    public class LayoutChangedEventArgs : EventArgs
    {
        public string OldLayoutId { get; }
        public string NewLayoutId { get; }
        public LayoutModel? NewLayout { get; }

        public LayoutChangedEventArgs(string oldLayoutId, string newLayoutId, LayoutModel? newLayout)
        {
            OldLayoutId = oldLayoutId;
            NewLayoutId = newLayoutId;
            NewLayout = newLayout;
        }
    }

    /// <summary>
    /// Event args for panel placement changed event
    /// </summary>
    public class PanelPlacementChangedEventArgs : EventArgs
    {
        public string PanelId { get; }
        public PanelPlacement Placement { get; }

        public PanelPlacementChangedEventArgs(string panelId, PanelPlacement placement)
        {
            PanelId = panelId;
            Placement = placement;
        }
    }
}
