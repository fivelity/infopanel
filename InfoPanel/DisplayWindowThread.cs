using InfoPanel.Models;
using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Threading;

namespace InfoPanel
{
    public class DisplayWindowThread
    {
        private readonly Profile _profile;
        private Thread? _thread;
        private readonly ManualResetEventSlim _readyEvent = new();

        public bool OpenGL;
        public event EventHandler<Guid>? WindowClosed;

        // Note: Window threading functionality will be re-implemented with WinUI 3
        // when the DisplayWindow class is ported to WinUI 3

        public DisplayWindowThread(Profile profile)
        {
            _profile = profile;
            OpenGL = profile.OpenGL;
        }

// Note: The following methods will be re-implemented when DisplayWindow is ported to WinUI 3
        // Current implementation is disabled as it relies on WPF Dispather
        
        // public void Start()
        // public void Show()
        // public void Close()
    }
}
