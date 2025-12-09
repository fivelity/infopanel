using System.Threading.Tasks;

namespace InfoPanel.Plugins
{
    /// <summary>
    /// Interface for layout plugins. Layout plugins provide structural arrangement rules
    /// (grid, canvas, regions) that can be loaded and applied to the application.
    /// </summary>
    public interface ILayoutPlugin : IPlugin
    {
        /// <summary>
        /// Get the layout model
        /// </summary>
        LayoutModel GetLayout();

        /// <summary>
        /// Load the layout asynchronously
        /// </summary>
        Task<LayoutModel> LoadLayoutAsync();

        /// <summary>
        /// Validate the layout
        /// </summary>
        bool ValidateLayout(LayoutModel layout);

        /// <summary>
        /// Get preview image path (optional)
        /// </summary>
        string? PreviewImagePath { get; }
    }
}
