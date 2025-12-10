using InfoPanel.Drawing;
using InfoPanel.Models;
using InfoPanel.Utils;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Win32;
using WinRT.Interop;
using Windows.Foundation;
using Windows.Graphics;
using SkiaSharp;
using SkiaSharp.Views.WinUI;
using System;
using Serilog;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.ComponentModel;

namespace InfoPanel.Views
{
    /// <summary>
    /// Display window for rendering profiles using SkiaSharp on WinUI 3
    /// </summary>
    public partial class DisplayWindow : Window
    {
        private static readonly ILogger Logger = Log.ForContext<DisplayWindow>();

        public Profile Profile { get; }
        public bool OpenGL { get; }

        private bool _dragMove = false;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly AppWindow _appWindow;
        private readonly IntPtr _hwnd;

        private bool _isUserResizing = false;
        private readonly DispatcherQueueTimer _resizeTimer;
        private bool _isDpiChanging = false;
        private bool _isClosed = false;

        private Timer? _renderTimer;
        private readonly FpsCounter FpsCounter = new();

        // For drag tracking
        private bool _isDragging = false;
        private Point _dragStartPoint;

        public DisplayWindow(Profile profile)
        {
            Profile = profile;
            OpenGL = profile.OpenGL;

            this.InitializeComponent();

            // Get window handle and AppWindow
            _hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            // Configure window appearance
            ConfigureWindowStyle();

            // Set initial size
            UpdateWindowSize();

            // Wire up events
            this.Activated += DisplayWindow_Activated;
            this.Closed += DisplayWindow_Closed;
            _appWindow.Changed += AppWindow_Changed;

            Profile.PropertyChanged += Profile_PropertyChanged;
            ConfigModel.Instance.Settings.PropertyChanged += Config_PropertyChanged;

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            _dispatcherQueue = this.DispatcherQueue;
            _resizeTimer = _dispatcherQueue.CreateTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(300);
            _resizeTimer.Tick += OnResizeCompleted;

            // Set up SkiaSharp canvas
            if (skElement != null)
            {
                skElement.Width = Profile.Width;
                skElement.Height = Profile.Height;
            }
        }

        private void ConfigureWindowStyle()
        {
            // Hide from Alt+Tab using Win32 API
            _ = SetWindowLong(_hwnd, GWL_EX_STYLE, (GetWindowLong(_hwnd, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

            // Configure presenter for borderless window
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = Profile.Resize;
                presenter.IsAlwaysOnTop = Profile.Topmost;
                presenter.SetBorderAndTitleBar(false, false);
            }
        }

        private void UpdateWindowSize()
        {
            // Set window size in physical pixels
            _appWindow.Resize(new SizeInt32(Profile.Width, Profile.Height));

            // Update SkiaSharp canvas size
            if (skElement != null)
            {
                skElement.Width = Profile.Width;
                skElement.Height = Profile.Height;
            }
        }

        private void DisplayWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                SetWindowPositionRelativeToScreen();
                UpdateSkiaTimer();
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange && !_isDpiChanging)
            {
                _isUserResizing = true;
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
        }

        private void DisplayWindow_Closed(object sender, WindowEventArgs args)
        {
            _isClosed = true;
            CleanupResources();
        }

        private void UpdateSkiaTimer()
        {
            double interval = (1000.0 / ConfigModel.Instance.Settings.TargetFrameRate) - 1;
            FpsCounter.SetMaxFrames(ConfigModel.Instance.Settings.TargetFrameRate);

            if (_renderTimer == null)
            {
                // Initialize the timer
                _renderTimer = new System.Timers.Timer(interval);
                _renderTimer.Elapsed += OnTimerElapsed;
                _renderTimer.AutoReset = true;
                _renderTimer.Start();
            }
            else
            {
                // Just update the interval if the timer already exists
                _renderTimer.Interval = interval;
            }
        }

        private void Clear(DisplayItem displayItem)
        {
            if (displayItem is GroupDisplayItem group)
            {
                foreach (var groupItem in group.DisplayItems)
                {
                    Clear(groupItem);
                }
            }
            else if (displayItem is GaugeDisplayItem gauge)
            {
                foreach (var gaugeItem in gauge.Images)
                {
                    Clear(gaugeItem);
                }
            }
            else if (displayItem is ImageDisplayItem image)
            {
                Cache.GetLocalImage(image, false)?.DisposeGLAssets();
            }
        }

        private void CleanupResources()
        {
            _resizeTimer.Stop();
            _resizeTimer.Tick -= OnResizeCompleted;

            ConfigModel.Instance.Settings.PropertyChanged -= Config_PropertyChanged;

            if (_renderTimer != null)
            {
                _renderTimer.Stop();
                _renderTimer.Elapsed -= OnTimerElapsed;
                _renderTimer.Dispose();
            }

            Profile.PropertyChanged -= Profile_PropertyChanged;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            _appWindow.Changed -= AppWindow_Changed;

            if (OpenGL)
            {
                foreach (var item in SharedModel.Instance.GetProfileDisplayItemsCopy(Profile))
                {
                    Clear(item);
                }
            }

            // TODO: Implement OpenGL cleanup when OpenGL support is added to WinUI 3 SkiaSharp
        }

        private void OnResizeCompleted(object? sender, object e)
        {
            _resizeTimer.Stop();

            if (_isUserResizing)
            {
                _isUserResizing = false;
                UpdateModelWithNewSize();
            }
        }

        private void UpdateModelWithNewSize()
        {
            // Get current window size from AppWindow
            var size = _appWindow.Size;
            Profile.Width = size.Width;
            Profile.Height = size.Height;

            // Update SkiaSharp canvas
            if (skElement != null)
            {
                skElement.Width = size.Width;
                skElement.Height = size.Height;
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_isClosed) return;
            _dispatcherQueue.TryEnqueue(() => skElement?.Invalidate());
        }

        private void SkElement_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            PaintSurface(e.Surface.Canvas);
        }

        private void PaintSurface(SKCanvas canvas)
        {
            if (_renderTimer == null || !_renderTimer.Enabled)
            {
                return;
            }

            canvas.Clear();

            SkiaGraphics skiaGraphics = new(canvas, Profile.FontScale);
            PanelDraw.Run(Profile, skiaGraphics, cacheHint: $"DISPLAY-{Profile.Guid}", fpsCounter: FpsCounter);
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            Logger.Information("SystemEvents_DisplaySettingsChanged");
            _dispatcherQueue.TryEnqueue(() =>
            {
                SetWindowPositionRelativeToScreen();
            });
        }

        public void Fullscreen()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                var screen = ScreenHelper.GetWindowScreen(this);
                if (screen != null)
                {
                    Profile.WindowX = 0;
                    Profile.WindowY = 0;
                    Profile.Width = (int)screen.Bounds.Width;
                    Profile.Height = (int)screen.Bounds.Height;
                }
            });
        }

        private void Config_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConfigModel.Instance.Settings.TargetFrameRate))
            {
                UpdateSkiaTimer();
            }
        }

        private void Profile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Profile.TargetWindow) || e.PropertyName == nameof(Profile.WindowX)
                                 || e.PropertyName == nameof(Profile.WindowY) || e.PropertyName == nameof(Profile.StrictWindowMatching))
            {
                if (!_dragMove)
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        SetWindowPositionRelativeToScreen();
                    });
                }
            }
            else if (e.PropertyName == nameof(Profile.Resize))
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (_appWindow.Presenter is OverlappedPresenter presenter)
                    {
                        presenter.IsResizable = Profile.Resize;
                    }
                });
            }
            else if (e.PropertyName == nameof(Profile.Topmost))
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (_appWindow.Presenter is OverlappedPresenter presenter)
                    {
                        presenter.IsAlwaysOnTop = Profile.Topmost;
                    }
                });
            }
            else if (e.PropertyName == nameof(Profile.Width) || e.PropertyName == nameof(Profile.Height))
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    UpdateWindowSize();
                });
            }
        }


        private void Window_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedVisibleItems != null)
            {
                foreach (var displayItem in SharedModel.Instance.SelectedVisibleItems)
                {
                    switch (e.Key)
                    {
                        case Windows.System.VirtualKey.Up:
                            displayItem.Y -= SharedModel.Instance.MoveValue;
                            break;
                        case Windows.System.VirtualKey.Down:
                            displayItem.Y += SharedModel.Instance.MoveValue;
                            break;
                        case Windows.System.VirtualKey.Left:
                            displayItem.X -= SharedModel.Instance.MoveValue;
                            break;
                        case Windows.System.VirtualKey.Right:
                            displayItem.X += SharedModel.Instance.MoveValue;
                            break;
                    }
                }
            }
        }

        private void Window_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            dragStart = false;
        }

        private void SetWindowPositionRelativeToScreen()
        {
            var screens = ScreenHelper.GetAllMonitors();
            MonitorInfo? targetScreen = null;

            if (Profile.TargetWindow is TargetWindow targetWindow)
            {
                targetScreen ??= screens.FirstOrDefault(s => s.DeviceName == targetWindow.DeviceName && s.Bounds.Width == targetWindow.Width && s.Bounds.Height == targetWindow.Height);

                if (!Profile.StrictWindowMatching)
                {
                    targetScreen ??= screens.FirstOrDefault(s => s.DeviceName == targetWindow.DeviceName);
                    targetScreen ??= screens.FirstOrDefault(s => s.Bounds.Width == targetWindow.Width && s.Bounds.Height == targetWindow.Height);
                }
            }

            if (!Profile.StrictWindowMatching)
            {
                targetScreen ??= screens.First();
            }

            if (targetScreen != null)
            {
                var x = targetScreen.Bounds.Left + Profile.WindowX;
                var y = targetScreen.Bounds.Top + Profile.WindowY;

                Log.Debug("SetWindowPositionRelativeToScreen targetScreen={TargetScreen}", targetScreen);
                Log.Debug("SetWindowPositionRelativeToScreen targetScreen={DeviceName} x={X} y={Y}", targetScreen.DeviceName, x, y);
                ScreenHelper.MoveWindowPhysical(this, (int)x, (int)y);
                
                // Show window if it was hidden
                if (!_appWindow.IsVisible)
                {
                    _appWindow.Show();
                }
            }
            else if (_appWindow.IsVisible)
            {
                _appWindow.Hide();
            }
        }

        private void Window_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pointerPoint = e.GetCurrentPoint((UIElement)sender);
            
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                if (!dragStart)
                {
                    var inSelectionBounds = false;
                    var position = pointerPoint.Position;
                    foreach (var displayItem in SharedModel.Instance.SelectedVisibleItems)
                    {
                        if (displayItem.ContainsPoint(position))
                        {
                            inSelectionBounds = true;
                            break;
                        }
                    }

                    if (!inSelectionBounds)
                    {
                        foreach (var selectedItem in SharedModel.Instance.SelectedVisibleItems)
                        {
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                selectedItem.Selected = false;
                            });
                        }
                    }
                }

                if (SharedModel.Instance.SelectedVisibleItems.Count == 0)
                {
                    if (Profile.Drag)
                    {
                        // Start window drag using Win32 API
                        StartWindowDrag();
                    }
                }
                else
                {
                    startPosition = pointerPoint.Position;

                    foreach (var item in SharedModel.Instance.SelectedVisibleItems)
                    {
                        item.MouseOffset = new Windows.Foundation.Point(startPosition.X - item.X, startPosition.Y - item.Y);
                    }

                    dragStart = true;
                }
            }
            else if (pointerPoint.Properties.IsMiddleButtonPressed)
            {
                if (SharedModel.Instance.SelectedProfile != Profile)
                {
                    return;
                }

                DisplayItem? clickedItem = null;
                var clickPosition = pointerPoint.Position;

                var displayItems = SharedModel.Instance.GetProfileDisplayItemsCopy(Profile).ToList();
                displayItems.Reverse();

                foreach (var item in displayItems)
                {
                    if (item.Hidden)
                    {
                        continue;
                    }

                    if (item is GroupDisplayItem groupDisplayItem)
                    {
                        var groupDisplayItems = groupDisplayItem.DisplayItemsCopy.ToList();
                        groupDisplayItems.Reverse();
                        foreach (var groupItem in groupDisplayItems)
                        {
                            if (groupItem.Hidden)
                            {
                                continue;
                            }

                            if (groupItem.ContainsPoint(clickPosition))
                            {
                                clickedItem = groupItem;
                                break;
                            }
                        }

                        if (clickedItem == null)
                        {
                            continue;
                        }
                    }

                    if (clickedItem != null)
                    {
                        break;
                    }

                    if (item.ContainsPoint(clickPosition))
                    {
                        clickedItem = item;
                        break;
                    }
                }

                if (clickedItem != null)
                {
                    // Check if shift or control is pressed
                    var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
                    var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
                    bool isShiftDown = (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;
                    bool isCtrlDown = (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;

                    if (!isShiftDown && !isCtrlDown)
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            SharedModel.Instance.SelectedItem = clickedItem;
                        });
                    }

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        clickedItem.Selected = true;
                    });
                }
                else
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        SharedModel.Instance.SelectedItem = null;
                    });
                }
            }
        }

        /// <summary>
        /// Starts window drag using Win32 API (replacement for WPF DragMove)
        /// </summary>
        private void StartWindowDrag()
        {
            // Send WM_NCLBUTTONDOWN with HTCAPTION to start drag
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int HTCAPTION = 2;

            ReleaseCapture();
            SendMessage(_hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);

            // After drag completes, update position
            var screen = ScreenHelper.GetWindowScreen(this);

            if (screen != null)
            {
                var position = ScreenHelper.GetWindowPositionPhysical(this);
                var relativePosition = ScreenHelper.GetWindowRelativePosition(screen, position);

                Log.Debug("SetPosition screen={Screen}", screen);
                Log.Debug("SetPosition screen={DeviceName} position={Position} relativePosition={RelativePosition}", screen.DeviceName, position, relativePosition);

                _dragMove = true;

                try
                {
                    Profile.TargetWindow = new TargetWindow((int)screen.Bounds.Left, (int)screen.Bounds.Top, (int)screen.Bounds.Width, (int)screen.Bounds.Height, screen.DeviceName);
                    Profile.WindowX = (int)relativePosition.X;
                    Profile.WindowY = (int)relativePosition.Y;
                }
                finally
                {
                    _dragMove = false;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        bool dragStart = false;
        global::Windows.Foundation.Point startPosition = new global::Windows.Foundation.Point();
        private void Window_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (dragStart)
            {

                if (SharedModel.Instance.SelectedVisibleItems.Count == 0)
                {
                    dragStart = false;
                    return;
                }

                var gridSize = SharedModel.Instance.MoveValue;

                var currentPosition = e.GetPosition((UIElement)sender);

                foreach (var displayItem in SharedModel.Instance.SelectedVisibleItems)
                {
                    if (displayItem.Selected && !displayItem.IsLocked)
                    {
                        int x = (int)(currentPosition.X - displayItem.MouseOffset.X);
                        int y = (int)(currentPosition.Y - displayItem.MouseOffset.Y);

                        x = (int)(Math.Round((double)x / gridSize) * gridSize);
                        y = (int)(Math.Round((double)y / gridSize) * gridSize);

                        displayItem.X = x;
                        displayItem.Y = y;
                    }
                }

                startPosition = currentPosition;
            }
        }

        private void MenuItemSavePosition_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ConfigModel.Instance.SaveProfiles();
        }

        private void MenuItemConfig_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Microsoft.UI.Xaml.Application.Current is App app)
            {
                app.ShowDesign(Profile);
            }
        }

        private void MenuItemClose_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Close();
        }

        //     [DllImport("user32.dll", SetLastError = true)]
        //     public static extern IntPtr FindWindowEx(IntPtr hP, IntPtr hC, string sC,
        //string sW);

        //     [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //     [return: MarshalAs(UnmanagedType.Bool)]
        //     public static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList
        //     lParam);

        //     public delegate bool EnumedWindow(IntPtr handleWindow, ArrayList handles);

        //     public static bool GetWindowHandle(IntPtr windowHandle, ArrayList
        //     windowHandles)
        //     {
        //         windowHandles.Add(windowHandle);
        //         return true;
        //     }

        //     private void SetAsDesktopChild()
        //     {
        //         ArrayList windowHandles = new ArrayList();
        //         EnumedWindow callBackPtr = GetWindowHandle;
        //         EnumWindows(callBackPtr, windowHandles);

        //         foreach (IntPtr windowHandle in windowHandles)
        //         {
        //             IntPtr hNextWin = FindWindowEx(windowHandle, IntPtr.Zero,
        //             "SHELLDLL_DefView", null);
        //             if (hNextWin != IntPtr.Zero)
        //             {
        //                 var interop = new WindowInteropHelper(this);
        //                 interop.EnsureHandle();
        //                 interop.Owner = hNextWin;
        //             }
        //         }
        //     }


        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;
    }
}

