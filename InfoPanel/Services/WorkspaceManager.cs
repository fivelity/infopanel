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
    /// Service for managing workspaces. Handles saving, loading, exporting, and importing
    /// workspace configurations.
    /// </summary>
    public class WorkspaceManager
    {
        private static WorkspaceManager? _instance;
        public static WorkspaceManager Instance => _instance ??= new WorkspaceManager();

        private readonly string _workspacesDirectory;
        private readonly string _workspacesFile;
        private WorkspaceCollection _workspaceCollection = new();

        public event EventHandler<WorkspaceChangedEventArgs>? WorkspaceChanged;
        public event EventHandler<WorkspacesLoadedEventArgs>? WorkspacesLoaded;

        public WorkspaceManager()
        {
            // Get workspaces directory from AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _workspacesDirectory = Path.Combine(appDataPath, "InfoPanel", "Workspaces");
            _workspacesFile = Path.Combine(_workspacesDirectory, "workspaces.json");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_workspacesDirectory))
            {
                Directory.CreateDirectory(_workspacesDirectory);
            }
        }

        /// <summary>
        /// Get all workspaces
        /// </summary>
        public List<WorkspaceModel> Workspaces => _workspaceCollection.Workspaces;

        /// <summary>
        /// Get the active workspace ID
        /// </summary>
        public string ActiveWorkspaceId
        {
            get => _workspaceCollection.ActiveWorkspaceId;
            set
            {
                if (_workspaceCollection.ActiveWorkspaceId != value)
                {
                    var oldWorkspaceId = _workspaceCollection.ActiveWorkspaceId;
                    _workspaceCollection.ActiveWorkspaceId = value;

                    var workspace = GetWorkspace(value);
                    if (workspace != null)
                    {
                        workspace.LastAccessedAt = DateTime.UtcNow;
                        WorkspaceChanged?.Invoke(this, new WorkspaceChangedEventArgs(oldWorkspaceId, value, workspace));
                    }
                }
            }
        }

        /// <summary>
        /// Get the active workspace
        /// </summary>
        public WorkspaceModel? ActiveWorkspace => GetWorkspace(ActiveWorkspaceId);

        /// <summary>
        /// Load workspaces from file
        /// </summary>
        public async Task LoadWorkspacesAsync()
        {
            if (File.Exists(_workspacesFile))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_workspacesFile);
                    var collection = JsonSerializer.Deserialize<WorkspaceCollection>(json);
                    if (collection != null)
                    {
                        _workspaceCollection = collection;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading workspaces: {ex.Message}");
                    CreateDefaultWorkspace();
                }
            }
            else
            {
                CreateDefaultWorkspace();
            }

            WorkspacesLoaded?.Invoke(this, new WorkspacesLoadedEventArgs(_workspaceCollection.Workspaces));
        }

        /// <summary>
        /// Save workspaces to file
        /// </summary>
        public async Task SaveWorkspacesAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_workspaceCollection, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_workspacesFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving workspaces: {ex.Message}");
            }
        }

        /// <summary>
        /// Create default workspace
        /// </summary>
        private void CreateDefaultWorkspace()
        {
            var defaultWorkspace = new WorkspaceModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Default Workspace",
                Description = "Default workspace configuration",
                ThemeId = "dark-default",
                LayoutId = "grid-standard",
                IsDefault = true
            };

            _workspaceCollection.Workspaces.Add(defaultWorkspace);
            _workspaceCollection.ActiveWorkspaceId = defaultWorkspace.Id;
        }

        /// <summary>
        /// Get workspace by ID
        /// </summary>
        public WorkspaceModel? GetWorkspace(string workspaceId)
        {
            return _workspaceCollection.Workspaces.FirstOrDefault(w => w.Id == workspaceId);
        }

        /// <summary>
        /// Create a new workspace
        /// </summary>
        public async Task<WorkspaceModel> CreateWorkspaceAsync(string name, string description = "")
        {
            var workspace = new WorkspaceModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                ThemeId = ThemeProvider.Instance.CurrentThemeId,
                LayoutId = LayoutProvider.Instance.CurrentLayoutId,
                PanelPlacements = LayoutProvider.Instance.PanelPlacements.Values.ToList()
            };

            _workspaceCollection.Workspaces.Add(workspace);
            await SaveWorkspacesAsync();
            return workspace;
        }

        /// <summary>
        /// Update a workspace
        /// </summary>
        public async Task UpdateWorkspaceAsync(WorkspaceModel workspace)
        {
            var existing = GetWorkspace(workspace.Id);
            if (existing != null)
            {
                var index = _workspaceCollection.Workspaces.IndexOf(existing);
                workspace.ModifiedAt = DateTime.UtcNow;
                _workspaceCollection.Workspaces[index] = workspace;
                await SaveWorkspacesAsync();
            }
        }

        /// <summary>
        /// Delete a workspace
        /// </summary>
        public async Task DeleteWorkspaceAsync(string workspaceId)
        {
            var workspace = GetWorkspace(workspaceId);
            if (workspace != null)
            {
                // Don't allow deleting the last workspace
                if (_workspaceCollection.Workspaces.Count <= 1)
                {
                    throw new InvalidOperationException("Cannot delete the last workspace");
                }

                // If deleting the default workspace, set another as default
                if (workspace.IsDefault)
                {
                    var newDefault = _workspaceCollection.Workspaces.FirstOrDefault(w => w.Id != workspaceId);
                    if (newDefault != null)
                    {
                        newDefault.IsDefault = true;
                    }
                }

                // If deleting the active workspace, switch to another
                if (workspaceId == ActiveWorkspaceId)
                {
                    var newActive = _workspaceCollection.Workspaces.FirstOrDefault(w => w.Id != workspaceId);
                    if (newActive != null)
                    {
                        ActiveWorkspaceId = newActive.Id;
                    }
                }

                _workspaceCollection.Workspaces.Remove(workspace);
                await SaveWorkspacesAsync();
            }
        }

        /// <summary>
        /// Set default workspace
        /// </summary>
        public async Task SetDefaultWorkspaceAsync(string workspaceId)
        {
            foreach (var workspace in _workspaceCollection.Workspaces)
            {
                workspace.IsDefault = workspace.Id == workspaceId;
            }
            await SaveWorkspacesAsync();
        }

        /// <summary>
        /// Apply a workspace (load theme, layout, and panel placements)
        /// </summary>
        public void ApplyWorkspace(WorkspaceModel workspace)
        {
            // Apply theme
            ThemeProvider.Instance.CurrentThemeId = workspace.ThemeId;

            // Apply layout
            LayoutProvider.Instance.CurrentLayoutId = workspace.LayoutId;

            // Apply panel placements
            LayoutProvider.Instance.LoadPanelPlacements(workspace.PanelPlacements);

            // Update last accessed
            workspace.LastAccessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Save current state to workspace
        /// </summary>
        public async Task SaveCurrentStateToWorkspaceAsync(string workspaceId)
        {
            var workspace = GetWorkspace(workspaceId);
            if (workspace != null)
            {
                workspace.ThemeId = ThemeProvider.Instance.CurrentThemeId;
                workspace.LayoutId = LayoutProvider.Instance.CurrentLayoutId;
                workspace.PanelPlacements = LayoutProvider.Instance.PanelPlacements.Values.ToList();
                workspace.ModifiedAt = DateTime.UtcNow;
                await SaveWorkspacesAsync();
            }
        }

        /// <summary>
        /// Export a workspace to a file
        /// </summary>
        public async Task ExportWorkspaceAsync(string workspaceId, string filePath)
        {
            var workspace = GetWorkspace(workspaceId);
            if (workspace != null)
            {
                var json = JsonSerializer.Serialize(workspace, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        /// <summary>
        /// Import a workspace from a file
        /// </summary>
        public async Task<WorkspaceModel?> ImportWorkspaceAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var workspace = JsonSerializer.Deserialize<WorkspaceModel>(json);
                if (workspace != null)
                {
                    // Generate new ID to avoid conflicts
                    workspace.Id = Guid.NewGuid().ToString();
                    workspace.CreatedAt = DateTime.UtcNow;
                    workspace.ModifiedAt = DateTime.UtcNow;
                    workspace.IsDefault = false;

                    _workspaceCollection.Workspaces.Add(workspace);
                    await SaveWorkspacesAsync();
                    return workspace;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing workspace: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Duplicate a workspace
        /// </summary>
        public async Task<WorkspaceModel> DuplicateWorkspaceAsync(string workspaceId, string newName)
        {
            var original = GetWorkspace(workspaceId);
            if (original == null)
            {
                throw new InvalidOperationException($"Workspace {workspaceId} not found");
            }

            var duplicate = new WorkspaceModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = newName,
                Description = original.Description,
                ThemeId = original.ThemeId,
                LayoutId = original.LayoutId,
                PanelPlacements = original.PanelPlacements.ToList(),
                IsDefault = false
            };

            _workspaceCollection.Workspaces.Add(duplicate);
            await SaveWorkspacesAsync();
            return duplicate;
        }
    }

    /// <summary>
    /// Event args for workspace changed event
    /// </summary>
    public class WorkspaceChangedEventArgs : EventArgs
    {
        public string OldWorkspaceId { get; }
        public string NewWorkspaceId { get; }
        public WorkspaceModel NewWorkspace { get; }

        public WorkspaceChangedEventArgs(string oldWorkspaceId, string newWorkspaceId, WorkspaceModel newWorkspace)
        {
            OldWorkspaceId = oldWorkspaceId;
            NewWorkspaceId = newWorkspaceId;
            NewWorkspace = newWorkspace;
        }
    }

    /// <summary>
    /// Event args for workspaces loaded event
    /// </summary>
    public class WorkspacesLoadedEventArgs : EventArgs
    {
        public List<WorkspaceModel> Workspaces { get; }

        public WorkspacesLoadedEventArgs(List<WorkspaceModel> workspaces)
        {
            Workspaces = workspaces;
        }
    }
}
