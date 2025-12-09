using InfoPanel.Models;
using System.Threading.Tasks;

namespace InfoPanel.Plugins
{
    /// <summary>
    /// Interface for theme plugins. Theme plugins provide visual styling (colors, typography,
    /// spacing, elevation, motion) that can be loaded and applied to the application.
    /// </summary>
    public interface IThemePlugin : IPlugin
    {
        /// <summary>
        /// Get the theme model
        /// </summary>
        ThemeModel GetTheme();

        /// <summary>
        /// Load the theme asynchronously
        /// </summary>
        Task<ThemeModel> LoadThemeAsync();

        /// <summary>
        /// Validate the theme
        /// </summary>
        bool ValidateTheme(ThemeModel theme);

        /// <summary>
        /// Get preview image path (optional)
        /// </summary>
        string? PreviewImagePath { get; }
    }
}
