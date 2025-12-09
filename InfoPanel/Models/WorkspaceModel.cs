using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InfoPanel.Models
{
    /// <summary>
    /// Represents a workspace configuration that ties together a theme, layout, and panel placements.
    /// Workspaces can be saved, loaded, exported, and imported.
    /// </summary>
    public class WorkspaceModel
    {
        /// <summary>
        /// Unique identifier for the workspace
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name of the workspace
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Default Workspace";

        /// <summary>
        /// Description of the workspace
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the theme ID
        /// </summary>
        [JsonPropertyName("themeId")]
        public string ThemeId { get; set; } = "dark-default";

        /// <summary>
        /// Reference to the layout ID
        /// </summary>
        [JsonPropertyName("layoutId")]
        public string LayoutId { get; set; } = "grid-standard";

        /// <summary>
        /// Panel placements in this workspace
        /// </summary>
        [JsonPropertyName("panelPlacements")]
        public List<PanelPlacement> PanelPlacements { get; set; } = new();

        /// <summary>
        /// Whether this is the default workspace
        /// </summary>
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; } = false;

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

        /// <summary>
        /// Last accessed timestamp
        /// </summary>
        [JsonPropertyName("lastAccessedAt")]
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Workspace metadata
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents the placement of a panel within a workspace
    /// </summary>
    public class PanelPlacement
    {
        /// <summary>
        /// Stable panel ID (references the panel instance)
        /// </summary>
        [JsonPropertyName("panelId")]
        public string PanelId { get; set; } = string.Empty;

        /// <summary>
        /// Region ID where the panel is placed
        /// </summary>
        [JsonPropertyName("regionId")]
        public string RegionId { get; set; } = string.Empty;

        /// <summary>
        /// X position within the region (for canvas layouts)
        /// </summary>
        [JsonPropertyName("x")]
        public double? X { get; set; }

        /// <summary>
        /// Y position within the region (for canvas layouts)
        /// </summary>
        [JsonPropertyName("y")]
        public double? Y { get; set; }

        /// <summary>
        /// Width of the panel
        /// </summary>
        [JsonPropertyName("width")]
        public double? Width { get; set; }

        /// <summary>
        /// Height of the panel
        /// </summary>
        [JsonPropertyName("height")]
        public double? Height { get; set; }

        /// <summary>
        /// Z-index for layering
        /// </summary>
        [JsonPropertyName("zIndex")]
        public int ZIndex { get; set; } = 0;

        /// <summary>
        /// Whether the panel is visible
        /// </summary>
        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Whether the panel is locked (cannot be moved/resized)
        /// </summary>
        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Custom properties for this panel placement
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Workspace collection for managing multiple workspaces
    /// </summary>
    public class WorkspaceCollection
    {
        [JsonPropertyName("workspaces")]
        public List<WorkspaceModel> Workspaces { get; set; } = new();

        [JsonPropertyName("activeWorkspaceId")]
        public string ActiveWorkspaceId { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";
    }
}
