using InfoPanel.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfoPanel.Plugins
{
    /// <summary>
    /// Base class for layout plugins. Provides default implementation for common functionality.
    /// </summary>
    public abstract class BaseLayoutPlugin : BasePlugin, ILayoutPlugin
    {
        protected LayoutModel? _layout;

        protected BaseLayoutPlugin(string id, string name, string description)
            : base(id, name, description)
        {
        }

        protected BaseLayoutPlugin(string name, string description = "")
            : base(name, description)
        {
        }

        public override string? ConfigFilePath => null;
        public override TimeSpan UpdateInterval => TimeSpan.Zero; // Layouts don't need updates

        public virtual string? PreviewImagePath => null;

        public abstract LayoutModel GetLayout();

        public virtual async Task<LayoutModel> LoadLayoutAsync()
        {
            return await Task.FromResult(GetLayout());
        }

        public virtual bool ValidateLayout(LayoutModel layout)
        {
            // Basic validation
            if (string.IsNullOrEmpty(layout.Id) || string.IsNullOrEmpty(layout.Name))
            {
                return false;
            }

            // Validate regions
            if (layout.Regions == null || layout.Regions.Count == 0)
            {
                return false;
            }

            // Validate grid or canvas definition based on type
            if (layout.Type == LayoutType.Grid && layout.Grid == null)
            {
                return false;
            }

            if (layout.Type == LayoutType.Canvas && layout.Canvas == null)
            {
                return false;
            }

            // All validations passed
            return true;
        }

        public override void Initialize()
        {
            _layout = GetLayout();
        }

        public override void Load(List<IPluginContainer> containers)
        {
            // Layouts don't use containers
        }

        public override void Update()
        {
            // Layouts don't need updates
        }

        public override Task UpdateAsync(CancellationToken cancellationToken)
        {
            // Layouts don't need updates
            return Task.CompletedTask;
        }

        public override void Close()
        {
            _layout = null;
        }
    }
}
