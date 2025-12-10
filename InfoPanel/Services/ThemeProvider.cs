using InfoPanel.Plugins;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace InfoPanel.Services
{
    /// <summary>
    /// Service for managing themes. Loads theme JSON files, applies tokenized ResourceDictionaries,
    /// and handles theme switching.
    /// </summary>
    public class ThemeProvider
    {
        private static ThemeProvider? _instance;
        public static ThemeProvider Instance => _instance ??= new ThemeProvider();

        private readonly Dictionary<string, ThemeModel> _themes = new();
        private string _currentThemeId = "dark-default";
        private readonly string _themesDirectory;

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public ThemeProvider()
        {
            // Get themes directory from AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _themesDirectory = Path.Combine(appDataPath, "InfoPanel", "Themes");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_themesDirectory))
            {
                Directory.CreateDirectory(_themesDirectory);
            }
        }

        /// <summary>
        /// Current theme ID
        /// </summary>
        public string CurrentThemeId
        {
            get => _currentThemeId;
            set
            {
                if (_currentThemeId != value)
                {
                    _currentThemeId = value;
                    ApplyTheme(value);
                }
            }
        }

        /// <summary>
        /// Get the current theme
        /// </summary>
        public ThemeModel? CurrentTheme => _themes.TryGetValue(_currentThemeId, out var theme) ? theme : null;

        /// <summary>
        /// Get all available themes
        /// </summary>
        public IReadOnlyDictionary<string, ThemeModel> Themes => _themes;

        /// <summary>
        /// Load all themes from the themes directory
        /// </summary>
        public async Task LoadThemesAsync()
        {
            _themes.Clear();

            // Load built-in default themes
            LoadDefaultThemes();
            Log.Information("ThemeProvider: Loaded {Count} built-in themes", _themes.Count);

            // Load themes from application directory (bundled themes)
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var appThemesDirectory = Path.Combine(appDirectory, "Themes");
            Log.Information("ThemeProvider: Checking for themes in {Directory}", appThemesDirectory);
            
            if (Directory.Exists(appThemesDirectory))
            {
                var themeFiles = Directory.GetFiles(appThemesDirectory, "*.json", SearchOption.AllDirectories);
                Log.Information("ThemeProvider: Found {Count} theme files", themeFiles.Length);
                
                foreach (var file in themeFiles)
                {
                    try
                    {
                        Log.Debug("ThemeProvider: Loading theme from {File}", file);
                        System.Diagnostics.Debug.WriteLine($"ThemeProvider: About to read file {file}");
                        // Use synchronous read to avoid async context issues during startup
                        var json = File.ReadAllText(file);
                        System.Diagnostics.Debug.WriteLine($"ThemeProvider: File read complete, length={json.Length}");
                        Log.Debug("ThemeProvider: Read JSON file, deserializing...");
                        var theme = JsonSerializer.Deserialize<ThemeModel>(json);
                        Log.Debug("ThemeProvider: Deserialized theme, checking validity...");
                        if (theme != null && !string.IsNullOrEmpty(theme.Id))
                        {
                            _themes[theme.Id] = theme;
                            Log.Information("ThemeProvider: Successfully loaded theme '{ThemeId}' from {File}", theme.Id, Path.GetFileName(file));
                        }
                        else
                        {
                            Log.Warning("ThemeProvider: Theme from {File} is null or has no ID", file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "ThemeProvider: Error loading theme from {File}", file);
                    }
                }
                Log.Information("ThemeProvider: Finished loading app themes");
            }
            else
            {
                Log.Warning("ThemeProvider: Themes directory does not exist: {Directory}", appThemesDirectory);
            }

            // Load custom themes from AppData directory
            if (Directory.Exists(_themesDirectory))
            {
                var themeFiles = Directory.GetFiles(_themesDirectory, "*.json", SearchOption.AllDirectories);
                foreach (var file in themeFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var theme = JsonSerializer.Deserialize<ThemeModel>(json);
                        if (theme != null && !string.IsNullOrEmpty(theme.Id))
                        {
                            _themes[theme.Id] = theme;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "ThemeProvider: Error loading custom theme from {File}", file);
                    }
                }
            }
            
            await Task.CompletedTask; // Keep method async-compatible
        }

        /// <summary>
        /// Apply a theme by ID
        /// </summary>
        public void ApplyTheme(string themeId)
        {
            if (!_themes.TryGetValue(themeId, out var theme))
            {
                // Fall back to default theme if not found
                themeId = "dark-default";
                if (!_themes.TryGetValue(themeId, out theme))
                {
                    return;
                }
            }

            // Create ResourceDictionary from theme
            var resourceDict = CreateResourceDictionary(theme);

            // Apply to application resources
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove old theme resources
                var oldThemeDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Contains("ThemeId"));
                if (oldThemeDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(oldThemeDict);
                }

                // Add new theme resources
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            });

            // Raise event
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }

        /// <summary>
        /// Create a ResourceDictionary from a theme model
        /// </summary>
        private ResourceDictionary CreateResourceDictionary(ThemeModel theme)
        {
            var dict = new ResourceDictionary();

            // Add theme ID for tracking
            dict["ThemeId"] = theme.Id;

            // Helper to add color and brush resources
            void AddColorResources(string themeKey, string dsKey, string colorString, System.Windows.Media.Color defaultColor)
            {
                // Parse color
                var color = Helpers.ColorHelper.ParseColor(colorString, defaultColor);
                
                // Add color resource
                dict[themeKey] = colorString; // Keep original string for reference
                dict[dsKey] = color;          // Add typed Color for DS token

                // Add brush resource
                var brush = new System.Windows.Media.SolidColorBrush(color);
                brush.Freeze();
                dict[$"{dsKey}Brush"] = brush;
                
                // For compatibility with Theme... keys if they were intended to be brushes
                // dict[$"{themeKey}Brush"] = brush; 
            }

            // Default colors for fallback (using dark theme defaults)
            var defaults = new ThemeModel().Colors; 

            // Add color tokens with DS mapping
            AddColorResources("ThemePrimaryColor", "DS_Primary", theme.Colors.Primary, Helpers.ColorHelper.ParseColor(defaults.Primary, System.Windows.Media.Colors.Blue));
            AddColorResources("ThemePrimaryHoverColor", "DS_PrimaryHover", theme.Colors.PrimaryHover, Helpers.ColorHelper.ParseColor(defaults.PrimaryHover, System.Windows.Media.Colors.DarkBlue));
            AddColorResources("ThemePrimaryPressedColor", "DS_PrimaryPressed", theme.Colors.PrimaryPressed, Helpers.ColorHelper.ParseColor(defaults.PrimaryPressed, System.Windows.Media.Colors.Navy));
            
            AddColorResources("ThemeBackgroundColor", "DS_Background", theme.Colors.Background, Helpers.ColorHelper.ParseColor(defaults.Background, System.Windows.Media.Colors.Black));
            AddColorResources("ThemeBackgroundSecondaryColor", "DS_BackgroundSecondary", theme.Colors.BackgroundSecondary, Helpers.ColorHelper.ParseColor(defaults.BackgroundSecondary, System.Windows.Media.Colors.Gray));
            AddColorResources("ThemeBackgroundTertiaryColor", "DS_BackgroundTertiary", theme.Colors.BackgroundTertiary, Helpers.ColorHelper.ParseColor(defaults.BackgroundTertiary, System.Windows.Media.Colors.DarkGray));
            
            // Map Surface to Card/Popover/Input as they usually share the same background in this design system
            AddColorResources("ThemeSurfaceColor", "DS_Card", theme.Colors.Surface, Helpers.ColorHelper.ParseColor(defaults.Surface, System.Windows.Media.Colors.Gray));
            AddColorResources("ThemeSurfaceColor", "DS_Popover", theme.Colors.Surface, Helpers.ColorHelper.ParseColor(defaults.Surface, System.Windows.Media.Colors.Gray));
            AddColorResources("ThemeSurfaceColor", "DS_Input", theme.Colors.Surface, Helpers.ColorHelper.ParseColor(defaults.Surface, System.Windows.Media.Colors.Gray));
            
            AddColorResources("ThemeSurfaceHoverColor", "DS_CardHover", theme.Colors.SurfaceHover, Helpers.ColorHelper.ParseColor(defaults.SurfaceHover, System.Windows.Media.Colors.LightGray));
            
            AddColorResources("ThemeTextPrimaryColor", "DS_Foreground", theme.Colors.TextPrimary, Helpers.ColorHelper.ParseColor(defaults.TextPrimary, System.Windows.Media.Colors.White));
            AddColorResources("ThemeTextSecondaryColor", "DS_ForegroundSecondary", theme.Colors.TextSecondary, Helpers.ColorHelper.ParseColor(defaults.TextSecondary, System.Windows.Media.Colors.LightGray));
            AddColorResources("ThemeTextTertiaryColor", "DS_ForegroundMuted", theme.Colors.TextTertiary, Helpers.ColorHelper.ParseColor(defaults.TextTertiary, System.Windows.Media.Colors.Gray));
            AddColorResources("ThemeTextDisabledColor", "DS_ForegroundDisabled", theme.Colors.TextDisabled, Helpers.ColorHelper.ParseColor(defaults.TextDisabled, System.Windows.Media.Colors.DarkGray));
            
            AddColorResources("ThemeAccentColor", "DS_Accent", theme.Colors.Accent, Helpers.ColorHelper.ParseColor(defaults.Accent, System.Windows.Media.Colors.Blue));
            AddColorResources("ThemeAccentHoverColor", "DS_AccentHover", theme.Colors.AccentHover, Helpers.ColorHelper.ParseColor(defaults.AccentHover, System.Windows.Media.Colors.DarkBlue));
            
            AddColorResources("ThemeSuccessColor", "DS_Success", theme.Colors.Success, Helpers.ColorHelper.ParseColor(defaults.Success, System.Windows.Media.Colors.Green));
            AddColorResources("ThemeWarningColor", "DS_Warning", theme.Colors.Warning, Helpers.ColorHelper.ParseColor(defaults.Warning, System.Windows.Media.Colors.Orange));
            AddColorResources("ThemeErrorColor", "DS_Destructive", theme.Colors.Error, Helpers.ColorHelper.ParseColor(defaults.Error, System.Windows.Media.Colors.Red));
            
            AddColorResources("ThemeBorderColor", "DS_Border", theme.Colors.Border, Helpers.ColorHelper.ParseColor(defaults.Border, System.Windows.Media.Colors.Gray));
            AddColorResources("ThemeBorderHoverColor", "DS_BorderHover", theme.Colors.BorderHover, Helpers.ColorHelper.ParseColor(defaults.BorderHover, System.Windows.Media.Colors.LightGray));
            
            // Explicitly set DS_Ring to Primary if not defined separately
            AddColorResources("ThemePrimaryColor", "DS_Ring", theme.Colors.Primary, Helpers.ColorHelper.ParseColor(defaults.Primary, System.Windows.Media.Colors.Blue));

            // Add typography tokens
            dict["ThemeFontFamily"] = theme.Typography.FontFamily;
            dict["ThemeFontFamilyMono"] = theme.Typography.FontFamilyMono;
            dict["ThemeFontSizeXs"] = theme.Typography.FontSizeXs;
            dict["ThemeFontSizeSm"] = theme.Typography.FontSizeSm;
            dict["ThemeFontSizeMd"] = theme.Typography.FontSizeMd;
            dict["ThemeFontSizeLg"] = theme.Typography.FontSizeLg;
            dict["ThemeFontSizeXl"] = theme.Typography.FontSizeXl;
            dict["ThemeFontSize2Xl"] = theme.Typography.FontSize2Xl;
            dict["ThemeFontSize3Xl"] = theme.Typography.FontSize3Xl;

            // Add spacing tokens
            dict["ThemeSpacingXs"] = theme.Spacing.Xs;
            dict["ThemeSpacingSm"] = theme.Spacing.Sm;
            dict["ThemeSpacingMd"] = theme.Spacing.Md;
            dict["ThemeSpacingLg"] = theme.Spacing.Lg;
            dict["ThemeSpacingXl"] = theme.Spacing.Xl;
            dict["ThemeSpacing2Xl"] = theme.Spacing.Xl2;
            dict["ThemeSpacing3Xl"] = theme.Spacing.Xl3;
            dict["ThemeRadiusSm"] = theme.Spacing.RadiusSm;
            dict["ThemeRadiusMd"] = theme.Spacing.RadiusMd;
            dict["ThemeRadiusLg"] = theme.Spacing.RadiusLg;
            dict["ThemeRadiusXl"] = theme.Spacing.RadiusXl;
            dict["ThemeRadiusFull"] = theme.Spacing.RadiusFull;

            // Add elevation tokens
            dict["ThemeShadow1"] = theme.Elevation.Shadow1;
            dict["ThemeShadow2"] = theme.Elevation.Shadow2;
            dict["ThemeShadow3"] = theme.Elevation.Shadow3;
            dict["ThemeShadow4"] = theme.Elevation.Shadow4;
            dict["ThemeShadow5"] = theme.Elevation.Shadow5;
            dict["ThemeZIndexBase"] = theme.Elevation.ZIndexBase;
            dict["ThemeZIndexDropdown"] = theme.Elevation.ZIndexDropdown;
            dict["ThemeZIndexModal"] = theme.Elevation.ZIndexModal;
            dict["ThemeZIndexTooltip"] = theme.Elevation.ZIndexTooltip;

            // Add motion tokens
            dict["ThemeDurationFast"] = theme.Motion.DurationFast;
            dict["ThemeDurationNormal"] = theme.Motion.DurationNormal;
            dict["ThemeDurationSlow"] = theme.Motion.DurationSlow;

            return dict;
        }

        /// <summary>
        /// Load built-in default themes
        /// </summary>
        private void LoadDefaultThemes()
        {
            // Dark theme (default)
            _themes["dark-default"] = new ThemeModel
            {
                Id = "dark-default",
                Name = "Dark (Default)",
                Description = "Default dark theme for InfoPanel",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Colors = new ColorTokens
                {
                    Primary = "#0078D4",
                    PrimaryHover = "#106EBE",
                    PrimaryPressed = "#005A9E",
                    Background = "#202020",
                    BackgroundSecondary = "#2C2C2C",
                    BackgroundTertiary = "#1A1A1A",
                    Surface = "#2C2C2C",
                    SurfaceHover = "#3C3C3C",
                    SurfacePressed = "#1C1C1C",
                    TextPrimary = "#FFFFFF",
                    TextSecondary = "#B3B3B3",
                    TextTertiary = "#808080",
                    TextDisabled = "#5C5C5C",
                    Accent = "#0078D4",
                    AccentHover = "#106EBE",
                    Success = "#10893E",
                    Warning = "#F7630C",
                    Error = "#E81123",
                    Info = "#0078D4",
                    Border = "#404040",
                    BorderHover = "#606060",
                    Divider = "#333333"
                }
            };

            // Light theme
            _themes["light-default"] = new ThemeModel
            {
                Id = "light-default",
                Name = "Light",
                Description = "Light theme for InfoPanel",
                Author = "InfoPanel Team",
                Version = "1.0.0",
                Colors = new ColorTokens
                {
                    Primary = "#0078D4",
                    PrimaryHover = "#106EBE",
                    PrimaryPressed = "#005A9E",
                    Background = "#FFFFFF",
                    BackgroundSecondary = "#F5F5F5",
                    BackgroundTertiary = "#FAFAFA",
                    Surface = "#FFFFFF",
                    SurfaceHover = "#F5F5F5",
                    SurfacePressed = "#E0E0E0",
                    TextPrimary = "#000000",
                    TextSecondary = "#5C5C5C",
                    TextTertiary = "#808080",
                    TextDisabled = "#B3B3B3",
                    Accent = "#0078D4",
                    AccentHover = "#106EBE",
                    Success = "#10893E",
                    Warning = "#F7630C",
                    Error = "#E81123",
                    Info = "#0078D4",
                    Border = "#D0D0D0",
                    BorderHover = "#A0A0A0",
                    Divider = "#E0E0E0"
                }
            };
        }

        /// <summary>
        /// Save a theme to a JSON file
        /// </summary>
        public async Task SaveThemeAsync(ThemeModel theme)
        {
            var filePath = Path.Combine(_themesDirectory, $"{theme.Id}.json");
            var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            _themes[theme.Id] = theme;
        }

        /// <summary>
        /// Export a theme to a file
        /// </summary>
        public async Task ExportThemeAsync(string themeId, string filePath)
        {
            if (_themes.TryGetValue(themeId, out var theme))
            {
                var json = JsonSerializer.Serialize(theme, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        /// <summary>
        /// Import a theme from a file
        /// </summary>
        public async Task<ThemeModel?> ImportThemeAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var theme = JsonSerializer.Deserialize<ThemeModel>(json);
                if (theme != null && !string.IsNullOrEmpty(theme.Id))
                {
                    await SaveThemeAsync(theme);
                    return theme;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing theme: {ex.Message}");
            }
            return null;
        }
    }

    /// <summary>
    /// Event args for theme changed event
    /// </summary>
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeModel Theme { get; }

        public ThemeChangedEventArgs(ThemeModel theme)
        {
            Theme = theme;
        }
    }
}
