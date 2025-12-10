using InfoPanel.Models;
using InfoPanel.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Dispatching;

namespace InfoPanel
{
    public class DisplayWindowManager
    {
        private static readonly Lazy<DisplayWindowManager> _instance = new(() => new DisplayWindowManager());
        public static DisplayWindowManager Instance => _instance.Value;

        private readonly Dictionary<Guid, DisplayWindow> _windows = new();
        private readonly object _lock = new();

        private DisplayWindowManager()
        {
            // WinUI 3 windows are created on the main thread
        }

        public void ShowDisplayWindow(Profile profile)
        {
            lock (_lock)
            {
                // Check if window exists
                if (_windows.TryGetValue(profile.Guid, out var existingWindow))
                {
                    // If Direct2D mode changed, close and recreate
                    if (existingWindow.OpenGL != profile.OpenGL)
                    {
                        existingWindow.Close();
                        _windows.Remove(profile.Guid);
                        CreateAndShowWindow(profile);
                    }
                    else
                    {
                        // Just show existing window
                        existingWindow.Activate();
                    }
                }
                else
                {
                    CreateAndShowWindow(profile);
                }
            }
        }

        private void CreateAndShowWindow(Profile profile)
        {
            var window = new DisplayWindow(profile);
            window.Closed += (s, e) => Window_Closed(window);
            _windows[profile.Guid] = window;
            window.Activate();
        }

        private void Window_Closed(DisplayWindow displayWindow)
        {
            lock (_lock)
            {
                _windows.Remove(displayWindow.Profile.Guid);
                displayWindow.Closed -= (s, e) => Window_Closed(displayWindow);
            }
        }

        public void CloseDisplayWindow(Guid profileGuid)
        {
            lock (_lock)
            {
                if (_windows.TryGetValue(profileGuid, out var window))
                {
                    window.Close();
                    _windows.Remove(profileGuid);
                }
            }
        }

        public DisplayWindow? GetWindow(Guid profileGuid)
        {
            lock (_lock)
            {
                _windows.TryGetValue(profileGuid, out var window);
                return window;
            }
        }

        public bool IsWindowOpen(Guid profileGuid)
        {
            lock (_lock)
            {
                return _windows.ContainsKey(profileGuid);
            }
        }

        public void CloseAll()
        {
            lock (_lock)
            {
                foreach (var window in _windows.Values)
                {
                    window.Close();
                }
                _windows.Clear();
            }
        }

        public void Dispose()
        {
            CloseAll();
        }

        // Optional: Allow thread to shut down when no windows are open
        public bool AllowThreadShutdown { get; set; } = false;
    }
}