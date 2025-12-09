using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InfoPanel.Models
{
    /// <summary>
    /// Registry for managing stable panel IDs and their metadata.
    /// Ensures panels can be tracked across layouts and workspaces.
    /// </summary>
    public class PanelRegistryModel
    {
        [JsonPropertyName("panels")]
        public List<PanelDescriptor> Panels { get; set; } = new();

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";
    }

    /// <summary>
    /// Descriptor for a registered panel
    /// </summary>
    public class PanelDescriptor
    {
        /// <summary>
        /// Stable unique identifier for the panel
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name of the panel
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Panel type (Sensor, InfoFeed, Control, Chart, Custom, etc.)
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Reference to sensor configuration (if type is Sensor)
        /// </summary>
        [JsonPropertyName("sensorId")]
        public string? SensorId { get; set; }

        /// <summary>
        /// Preferred width
        /// </summary>
        [JsonPropertyName("preferredWidth")]
        public double? PreferredWidth { get; set; }

        /// <summary>
        /// Preferred height
        /// </summary>
        [JsonPropertyName("preferredHeight")]
        public double? PreferredHeight { get; set; }

        /// <summary>
        /// Panel metadata
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Created timestamp
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
