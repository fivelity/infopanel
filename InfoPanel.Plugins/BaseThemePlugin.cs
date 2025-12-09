using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfoPanel.Plugins
{
    /// <summary>
    /// Base class for theme plugins. Provides default implementation for common functionality.
    /// </summary>
    public abstract class BaseThemePlugin : BasePlugin, IThemePlugin
    {
        protected ThemeModel? _theme;

        protected BaseThemePlugin(string id, string name, string description)
            : base(id, name, description)
        {
        }

        protected BaseThemePlugin(string name, string description = "")
            : base(name, description)
        {
        }

        public override string? ConfigFilePath => null;
        public override TimeSpan UpdateInterval => TimeSpan.Zero; // Themes don't need updates

        public virtual string? PreviewImagePath => null;

        public abstract ThemeModel GetTheme();

        public virtual async Task<ThemeModel> LoadThemeAsync()
        {
            return await Task.FromResult(GetTheme());
        }

        public virtual bool ValidateTheme(ThemeModel theme)
        {
            // Basic validation
            if (string.IsNullOrEmpty(theme.Id) || string.IsNullOrEmpty(theme.Name))
            {
                return false;
            }

            // Validate color tokens
            if (theme.Colors == null)
            {
                return false;
            }

            // All validations passed
            return true;
        }

        public override void Initialize()
        {
            _theme = GetTheme();
        }

        public override void Load(List<IPluginContainer> containers)
        {
            // Themes don't use containers
        }

        public override void Update()
        {
            // Themes don't need updates
        }

        public override Task UpdateAsync(CancellationToken cancellationToken)
        {
            // Themes don't need updates
            return Task.CompletedTask;
        }

        public override void Close()
        {
            _theme = null;
        }
    }
}
