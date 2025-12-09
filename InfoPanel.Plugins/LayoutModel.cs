using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InfoPanel.Plugins
{
    /// <summary>
    /// Represents a layout with structural placement rules (grid, regions, docking).
    /// Layouts are loadable, swappable plugins that define how panels are arranged.
    /// </summary>
    public class LayoutModel
    {
        /// <summary>
        /// Unique identifier for the layout (e.g., "grid-standard", "canvas-free")
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the layout
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the layout
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Layout author
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; } = "Unknown";

        /// <summary>
        /// Layout version (semantic versioning)
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Layout type (Grid, Canvas, Dock, etc.)
        /// </summary>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LayoutType Type { get; set; } = LayoutType.Grid;

        /// <summary>
        /// Grid definition (if Type is Grid)
        /// </summary>
        [JsonPropertyName("grid")]
        public GridDefinition? Grid { get; set; }

        /// <summary>
        /// Canvas definition (if Type is Canvas)
        /// </summary>
        [JsonPropertyName("canvas")]
        public CanvasDefinition? Canvas { get; set; }

        /// <summary>
        /// Regions for panel placement
        /// </summary>
        [JsonPropertyName("regions")]
        public List<LayoutRegion> Regions { get; set; } = new();

        /// <summary>
        /// Snapping settings
        /// </summary>
        [JsonPropertyName("snapping")]
        public SnappingSettings Snapping { get; set; } = new();

        /// <summary>
        /// Constraints for panel placement
        /// </summary>
        [JsonPropertyName("constraints")]
        public LayoutConstraints Constraints { get; set; } = new();

        /// <summary>
        /// Created timestamp
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Layout types
    /// </summary>
    public enum LayoutType
    {
        Grid,
        Canvas,
        Dock,
        Stack
    }

    /// <summary>
    /// Grid definition for grid-based layouts
    /// </summary>
    public class GridDefinition
    {
        [JsonPropertyName("rows")]
        public int Rows { get; set; } = 3;

        [JsonPropertyName("columns")]
        public int Columns { get; set; } = 3;

        [JsonPropertyName("rowDefinitions")]
        public List<string> RowDefinitions { get; set; } = new() { "*", "*", "*" };

        [JsonPropertyName("columnDefinitions")]
        public List<string> ColumnDefinitions { get; set; } = new() { "*", "*", "*" };

        [JsonPropertyName("gap")]
        public double Gap { get; set; } = 8;

        [JsonPropertyName("padding")]
        public double Padding { get; set; } = 16;
    }

    /// <summary>
    /// Canvas definition for free-form layouts
    /// </summary>
    public class CanvasDefinition
    {
        [JsonPropertyName("width")]
        public double Width { get; set; } = 1920;

        [JsonPropertyName("height")]
        public double Height { get; set; } = 1080;

        [JsonPropertyName("allowOverlap")]
        public bool AllowOverlap { get; set; } = true;

        [JsonPropertyName("snapToGrid")]
        public bool SnapToGrid { get; set; } = true;

        [JsonPropertyName("gridSize")]
        public double GridSize { get; set; } = 8;
    }

    /// <summary>
    /// Represents a region where panels can be placed
    /// </summary>
    public class LayoutRegion
    {
        /// <summary>
        /// Unique identifier for the region
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the region
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Grid row (for grid layouts)
        /// </summary>
        [JsonPropertyName("row")]
        public int? Row { get; set; }

        /// <summary>
        /// Grid column (for grid layouts)
        /// </summary>
        [JsonPropertyName("column")]
        public int? Column { get; set; }

        /// <summary>
        /// Row span (for grid layouts)
        /// </summary>
        [JsonPropertyName("rowSpan")]
        public int RowSpan { get; set; } = 1;

        /// <summary>
        /// Column span (for grid layouts)
        /// </summary>
        [JsonPropertyName("columnSpan")]
        public int ColumnSpan { get; set; } = 1;

        /// <summary>
        /// X position (for canvas layouts)
        /// </summary>
        [JsonPropertyName("x")]
        public double? X { get; set; }

        /// <summary>
        /// Y position (for canvas layouts)
        /// </summary>
        [JsonPropertyName("y")]
        public double? Y { get; set; }

        /// <summary>
        /// Width of the region
        /// </summary>
        [JsonPropertyName("width")]
        public double? Width { get; set; }

        /// <summary>
        /// Height of the region
        /// </summary>
        [JsonPropertyName("height")]
        public double? Height { get; set; }

        /// <summary>
        /// Minimum width
        /// </summary>
        [JsonPropertyName("minWidth")]
        public double? MinWidth { get; set; }

        /// <summary>
        /// Minimum height
        /// </summary>
        [JsonPropertyName("minHeight")]
        public double? MinHeight { get; set; }

        /// <summary>
        /// Maximum width
        /// </summary>
        [JsonPropertyName("maxWidth")]
        public double? MaxWidth { get; set; }

        /// <summary>
        /// Maximum height
        /// </summary>
        [JsonPropertyName("maxHeight")]
        public double? MaxHeight { get; set; }

        /// <summary>
        /// Whether this region can contain multiple panels
        /// </summary>
        [JsonPropertyName("allowMultiplePanels")]
        public bool AllowMultiplePanels { get; set; } = false;

        /// <summary>
        /// Whether panels in this region can be resized
        /// </summary>
        [JsonPropertyName("allowResize")]
        public bool AllowResize { get; set; } = true;

        /// <summary>
        /// Whether panels in this region can be moved
        /// </summary>
        [JsonPropertyName("allowMove")]
        public bool AllowMove { get; set; } = true;

        /// <summary>
        /// Z-index for layering
        /// </summary>
        [JsonPropertyName("zIndex")]
        public int ZIndex { get; set; } = 0;
    }

    /// <summary>
    /// Snapping settings for panel placement
    /// </summary>
    public class SnappingSettings
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("snapToGrid")]
        public bool SnapToGrid { get; set; } = true;

        [JsonPropertyName("snapToRegions")]
        public bool SnapToRegions { get; set; } = true;

        [JsonPropertyName("snapToPanels")]
        public bool SnapToPanels { get; set; } = true;

        [JsonPropertyName("snapThreshold")]
        public double SnapThreshold { get; set; } = 8;
    }

    /// <summary>
    /// Layout constraints
    /// </summary>
    public class LayoutConstraints
    {
        [JsonPropertyName("minPanelWidth")]
        public double MinPanelWidth { get; set; } = 50;

        [JsonPropertyName("minPanelHeight")]
        public double MinPanelHeight { get; set; } = 50;

        [JsonPropertyName("maxPanelWidth")]
        public double? MaxPanelWidth { get; set; }

        [JsonPropertyName("maxPanelHeight")]
        public double? MaxPanelHeight { get; set; }

        [JsonPropertyName("preserveAspectRatio")]
        public bool PreserveAspectRatio { get; set; } = false;

        [JsonPropertyName("allowOverlap")]
        public bool AllowOverlap { get; set; } = false;

        [JsonPropertyName("allowOutOfBounds")]
        public bool AllowOutOfBounds { get; set; } = false;
    }
}
