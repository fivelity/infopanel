using InfoPanel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace InfoPanel.Services
{
    /// <summary>
    /// Service for managing the panel registry. Maps stable panel IDs to instances
    /// and ensures panels are discoverable for layout placement.
    /// </summary>
    public class PanelRegistry
    {
        private static PanelRegistry? _instance;
        public static PanelRegistry Instance => _instance ??= new PanelRegistry();

        private readonly string _registryFile;
        private PanelRegistryModel _registry = new();
        private readonly Dictionary<string, object> _panelInstances = new();

        public event EventHandler<PanelRegisteredEventArgs>? PanelRegistered;
        public event EventHandler<PanelUnregisteredEventArgs>? PanelUnregistered;

        public PanelRegistry()
        {
            // Get registry file from AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var registryDirectory = Path.Combine(appDataPath, "InfoPanel");
            _registryFile = Path.Combine(registryDirectory, "panel-registry.json");

            // Create directory if it doesn't exist
            if (!Directory.Exists(registryDirectory))
            {
                Directory.CreateDirectory(registryDirectory);
            }
        }

        /// <summary>
        /// Get all registered panels
        /// </summary>
        public IReadOnlyList<PanelDescriptor> Panels => _registry.Panels;

        /// <summary>
        /// Load the panel registry from file
        /// </summary>
        public async Task LoadRegistryAsync()
        {
            if (File.Exists(_registryFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_registryFile);
                    var registry = JsonSerializer.Deserialize<PanelRegistryModel>(json);
                    if (registry != null)
                    {
                        _registry = registry;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading panel registry: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Save the panel registry to file
        /// </summary>
        public async Task SaveRegistryAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_registry, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_registryFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving panel registry: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a panel
        /// </summary>
        public async Task<string> RegisterPanelAsync(string name, string type, object instance, string? sensorId = null)
        {
            var descriptor = new PanelDescriptor
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Type = type,
                SensorId = sensorId
            };

            _registry.Panels.Add(descriptor);
            _panelInstances[descriptor.Id] = instance;

            await SaveRegistryAsync();
            PanelRegistered?.Invoke(this, new PanelRegisteredEventArgs(descriptor));

            return descriptor.Id;
        }

        /// <summary>
        /// Unregister a panel
        /// </summary>
        public async Task UnregisterPanelAsync(string panelId)
        {
            var descriptor = GetPanelDescriptor(panelId);
            if (descriptor != null)
            {
                _registry.Panels.Remove(descriptor);
                _panelInstances.Remove(panelId);

                await SaveRegistryAsync();
                PanelUnregistered?.Invoke(this, new PanelUnregisteredEventArgs(panelId));
            }
        }

        /// <summary>
        /// Get panel descriptor by ID
        /// </summary>
        public PanelDescriptor? GetPanelDescriptor(string panelId)
        {
            return _registry.Panels.FirstOrDefault(p => p.Id == panelId);
        }

        /// <summary>
        /// Get panel instance by ID
        /// </summary>
        public object? GetPanelInstance(string panelId)
        {
            return _panelInstances.TryGetValue(panelId, out var instance) ? instance : null;
        }

        /// <summary>
        /// Get panel instance by ID with type cast
        /// </summary>
        public T? GetPanelInstance<T>(string panelId) where T : class
        {
            return GetPanelInstance(panelId) as T;
        }

        /// <summary>
        /// Update panel descriptor
        /// </summary>
        public async Task UpdatePanelDescriptorAsync(PanelDescriptor descriptor)
        {
            var existing = GetPanelDescriptor(descriptor.Id);
            if (existing != null)
            {
                var index = _registry.Panels.IndexOf(existing);
                _registry.Panels[index] = descriptor;
                await SaveRegistryAsync();
            }
        }

        /// <summary>
        /// Get panels by type
        /// </summary>
        public IEnumerable<PanelDescriptor> GetPanelsByType(string type)
        {
            return _registry.Panels.Where(p => p.Type == type);
        }

        /// <summary>
        /// Get panel by sensor ID
        /// </summary>
        public PanelDescriptor? GetPanelBySensorId(string sensorId)
        {
            return _registry.Panels.FirstOrDefault(p => p.SensorId == sensorId);
        }

        /// <summary>
        /// Clear all panels (use with caution!)
        /// </summary>
        public async Task ClearRegistryAsync()
        {
            _registry.Panels.Clear();
            _panelInstances.Clear();
            await SaveRegistryAsync();
        }

        /// <summary>
        /// Associate a panel instance with a registered panel ID
        /// </summary>
        public void AssociatePanelInstance(string panelId, object instance)
        {
            _panelInstances[panelId] = instance;
        }

        /// <summary>
        /// Remove panel instance association
        /// </summary>
        public void DisassociatePanelInstance(string panelId)
        {
            _panelInstances.Remove(panelId);
        }
    }

    /// <summary>
    /// Event args for panel registered event
    /// </summary>
    public class PanelRegisteredEventArgs : EventArgs
    {
        public PanelDescriptor Descriptor { get; }

        public PanelRegisteredEventArgs(PanelDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
    }

    /// <summary>
    /// Event args for panel unregistered event
    /// </summary>
    public class PanelUnregisteredEventArgs : EventArgs
    {
        public string PanelId { get; }

        public PanelUnregisteredEventArgs(string panelId)
        {
            PanelId = panelId;
        }
    }
}
