using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InfoPanel.Plugins
{
    /// <summary>
    /// Represents a theme with visual tokens (colors, typography, spacing, elevation, motion).
    /// Themes are loadable, swappable plugins that define the look and feel of the application.
    /// </summary>
    public class ThemeModel
    {
        /// <summary>
        /// Unique identifier for the theme (e.g., "dark-default", "light-modern")
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the theme
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the theme
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Theme author
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; } = "Unknown";

        /// <summary>
        /// Theme version (semantic versioning)
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Color tokens
        /// </summary>
        [JsonPropertyName("colors")]
        public ColorTokens Colors { get; set; } = new();

        /// <summary>
        /// Typography tokens
        /// </summary>
        [JsonPropertyName("typography")]
        public TypographyTokens Typography { get; set; } = new();

        /// <summary>
        /// Spacing tokens (in pixels)
        /// </summary>
        [JsonPropertyName("spacing")]
        public SpacingTokens Spacing { get; set; } = new();

        /// <summary>
        /// Elevation/shadow tokens
        /// </summary>
        [JsonPropertyName("elevation")]
        public ElevationTokens Elevation { get; set; } = new();

        /// <summary>
        /// Motion/animation tokens
        /// </summary>
        [JsonPropertyName("motion")]
        public MotionTokens Motion { get; set; } = new();

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
    /// Color tokens for the theme
    /// </summary>
    public class ColorTokens
    {
        // Primary colors
        [JsonPropertyName("primary")]
        public string Primary { get; set; } = "#0078D4";

        [JsonPropertyName("primaryHover")]
        public string PrimaryHover { get; set; } = "#106EBE";

        [JsonPropertyName("primaryPressed")]
        public string PrimaryPressed { get; set; } = "#005A9E";

        // Background colors
        [JsonPropertyName("background")]
        public string Background { get; set; } = "#202020";

        [JsonPropertyName("backgroundSecondary")]
        public string BackgroundSecondary { get; set; } = "#2C2C2C";

        [JsonPropertyName("backgroundTertiary")]
        public string BackgroundTertiary { get; set; } = "#1A1A1A";

        // Surface colors
        [JsonPropertyName("surface")]
        public string Surface { get; set; } = "#2C2C2C";

        [JsonPropertyName("surfaceHover")]
        public string SurfaceHover { get; set; } = "#3C3C3C";

        [JsonPropertyName("surfacePressed")]
        public string SurfacePressed { get; set; } = "#1C1C1C";

        // Text colors
        [JsonPropertyName("textPrimary")]
        public string TextPrimary { get; set; } = "#FFFFFF";

        [JsonPropertyName("textSecondary")]
        public string TextSecondary { get; set; } = "#B3B3B3";

        [JsonPropertyName("textTertiary")]
        public string TextTertiary { get; set; } = "#808080";

        [JsonPropertyName("textDisabled")]
        public string TextDisabled { get; set; } = "#5C5C5C";

        // Accent colors
        [JsonPropertyName("accent")]
        public string Accent { get; set; } = "#0078D4";

        [JsonPropertyName("accentHover")]
        public string AccentHover { get; set; } = "#106EBE";

        // Status colors
        [JsonPropertyName("success")]
        public string Success { get; set; } = "#10893E";

        [JsonPropertyName("warning")]
        public string Warning { get; set; } = "#F7630C";

        [JsonPropertyName("error")]
        public string Error { get; set; } = "#E81123";

        [JsonPropertyName("info")]
        public string Info { get; set; } = "#0078D4";

        // Border colors
        [JsonPropertyName("border")]
        public string Border { get; set; } = "#404040";

        [JsonPropertyName("borderHover")]
        public string BorderHover { get; set; } = "#606060";

        [JsonPropertyName("divider")]
        public string Divider { get; set; } = "#333333";
    }

    /// <summary>
    /// Typography tokens for the theme
    /// </summary>
    public class TypographyTokens
    {
        [JsonPropertyName("fontFamily")]
        public string FontFamily { get; set; } = "Segoe UI, Arial, sans-serif";

        [JsonPropertyName("fontFamilyMono")]
        public string FontFamilyMono { get; set; } = "Consolas, Courier New, monospace";

        // Font sizes (in pixels)
        [JsonPropertyName("fontSizeXs")]
        public double FontSizeXs { get; set; } = 10;

        [JsonPropertyName("fontSizeSm")]
        public double FontSizeSm { get; set; } = 12;

        [JsonPropertyName("fontSizeMd")]
        public double FontSizeMd { get; set; } = 14;

        [JsonPropertyName("fontSizeLg")]
        public double FontSizeLg { get; set; } = 16;

        [JsonPropertyName("fontSizeXl")]
        public double FontSizeXl { get; set; } = 20;

        [JsonPropertyName("fontSize2Xl")]
        public double FontSize2Xl { get; set; } = 24;

        [JsonPropertyName("fontSize3Xl")]
        public double FontSize3Xl { get; set; } = 32;

        // Font weights
        [JsonPropertyName("fontWeightLight")]
        public string FontWeightLight { get; set; } = "Light";

        [JsonPropertyName("fontWeightNormal")]
        public string FontWeightNormal { get; set; } = "Normal";

        [JsonPropertyName("fontWeightMedium")]
        public string FontWeightMedium { get; set; } = "Medium";

        [JsonPropertyName("fontWeightSemiBold")]
        public string FontWeightSemiBold { get; set; } = "SemiBold";

        [JsonPropertyName("fontWeightBold")]
        public string FontWeightBold { get; set; } = "Bold";

        // Line heights
        [JsonPropertyName("lineHeightTight")]
        public double LineHeightTight { get; set; } = 1.2;

        [JsonPropertyName("lineHeightNormal")]
        public double LineHeightNormal { get; set; } = 1.5;

        [JsonPropertyName("lineHeightRelaxed")]
        public double LineHeightRelaxed { get; set; } = 1.75;
    }

    /// <summary>
    /// Spacing tokens for the theme (in pixels)
    /// </summary>
    public class SpacingTokens
    {
        [JsonPropertyName("xs")]
        public double Xs { get; set; } = 4;

        [JsonPropertyName("sm")]
        public double Sm { get; set; } = 8;

        [JsonPropertyName("md")]
        public double Md { get; set; } = 16;

        [JsonPropertyName("lg")]
        public double Lg { get; set; } = 24;

        [JsonPropertyName("xl")]
        public double Xl { get; set; } = 32;

        [JsonPropertyName("2xl")]
        public double Xl2 { get; set; } = 48;

        [JsonPropertyName("3xl")]
        public double Xl3 { get; set; } = 64;

        // Corner radius
        [JsonPropertyName("radiusSm")]
        public double RadiusSm { get; set; } = 2;

        [JsonPropertyName("radiusMd")]
        public double RadiusMd { get; set; } = 4;

        [JsonPropertyName("radiusLg")]
        public double RadiusLg { get; set; } = 8;

        [JsonPropertyName("radiusXl")]
        public double RadiusXl { get; set; } = 12;

        [JsonPropertyName("radiusFull")]
        public double RadiusFull { get; set; } = 9999;
    }

    /// <summary>
    /// Elevation/shadow tokens for the theme
    /// </summary>
    public class ElevationTokens
    {
        [JsonPropertyName("shadow1")]
        public string Shadow1 { get; set; } = "0 1px 2px rgba(0,0,0,0.3)";

        [JsonPropertyName("shadow2")]
        public string Shadow2 { get; set; } = "0 2px 4px rgba(0,0,0,0.3)";

        [JsonPropertyName("shadow3")]
        public string Shadow3 { get; set; } = "0 4px 8px rgba(0,0,0,0.3)";

        [JsonPropertyName("shadow4")]
        public string Shadow4 { get; set; } = "0 8px 16px rgba(0,0,0,0.3)";

        [JsonPropertyName("shadow5")]
        public string Shadow5 { get; set; } = "0 16px 32px rgba(0,0,0,0.4)";

        // Z-index layers
        [JsonPropertyName("zIndexBase")]
        public int ZIndexBase { get; set; } = 0;

        [JsonPropertyName("zIndexDropdown")]
        public int ZIndexDropdown { get; set; } = 1000;

        [JsonPropertyName("zIndexModal")]
        public int ZIndexModal { get; set; } = 2000;

        [JsonPropertyName("zIndexTooltip")]
        public int ZIndexTooltip { get; set; } = 3000;
    }

    /// <summary>
    /// Motion/animation tokens for the theme
    /// </summary>
    public class MotionTokens
    {
        // Duration (in milliseconds)
        [JsonPropertyName("durationFast")]
        public int DurationFast { get; set; } = 100;

        [JsonPropertyName("durationNormal")]
        public int DurationNormal { get; set; } = 200;

        [JsonPropertyName("durationSlow")]
        public int DurationSlow { get; set; } = 300;

        // Easing functions
        [JsonPropertyName("easingStandard")]
        public string EasingStandard { get; set; } = "cubic-bezier(0.4, 0.0, 0.2, 1)";

        [JsonPropertyName("easingDecelerate")]
        public string EasingDecelerate { get; set; } = "cubic-bezier(0.0, 0.0, 0.2, 1)";

        [JsonPropertyName("easingAccelerate")]
        public string EasingAccelerate { get; set; } = "cubic-bezier(0.4, 0.0, 1, 1)";
    }
}
