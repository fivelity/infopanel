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
        // TODO: Replace DisplayWindow with WinUI 3 equivalent
        // private DisplayWindow? _window;
        // TODO: Replace WPF Dispatcher with WinUI 3 equivalent
        // private Dispatcher? _dispatcher;
        private readonly ManualResetEventSlim _readyEvent = new();

        // TODO: Re-enable when DisplayWindow is ported to WinUI 3
        // public DisplayWindow? Window => _window;

        public bool OpenGL;
        public event EventHandler<Guid>? WindowClosed;

        // TODO: Re-enable when DisplayWindow is ported to WinUI 3
        // public DisplayWindowThread(Profile profile)
        // {
        //     _profile = profile;
        //     StartUIThread();
        // }

        public DisplayWindowThread(Profile profile)
        {
            _profile = profile;
            OpenGL = profile.OpenGL;
        }

        // TODO: Re-enable when DisplayWindow is ported to WinUI 3
        // public void Start()
        // {
        //     _thread = new Thread(ThreadMain)
        //     {
        //         IsBackground = false
        //     };
        //     _thread.SetApartmentState(ApartmentState.STA);
        //     _thread.Start();
        //     _readyEvent.Wait(5000);
        // }

        // TODO: Re-enable when DisplayWindow is ported to WinUI 3
        // private void ThreadMain()
        // {
        //     _window = new DisplayWindow(_profile);
        //     _dispatcher = _window.Dispatcher;
        //     _window.Closed += (s, e) =>
        //     {
        //         WindowClosed?.Invoke(this, _profile.Guid);
        //         _dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
        //     };
        //     _readyEvent.Set();
        //     _window.Show();
        //     Dispatcher.Run();
        // }

        // TODO: Re-enable when DisplayWindow is ported to WinUI 3
        // public void Show()
        // {
        //     _dispatcher?.BeginInvoke(() => _window?.Show());
        // }

        // TODO: Re-enable when DisplayWindow is ported to WinUI 3
        // public void Close()
        // {
        //     _dispatcher?.BeginInvoke(() => _window?.Close());
        //     _thread?.Join(2000);
        // }
    }
}
