using InfoPanel.Drawing;
using InfoPanel.Models;
using InfoPanel.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;
using Windows.Foundation;
using SkiaSharp;
using SkiaSharp.Views.WinUI;
using System;
using Serilog;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace InfoPanel.Views
{
    /// <summary>
    /// Interaction logic for DisplayWindow.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {
        private static readonly ILogger Logger = Log.ForContext<DisplayWindow>();
        private SKXamlCanvas? _sKElement;

        public Profile Profile { get; }
        public bool OpenGL { get; }

        private bool _dragMove = false;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

        private bool _isUserResizing = false;
        private readonly Microsoft.UI.Dispatching.DispatcherQueueTimer _resizeTimer;
        private bool _isDpiChanging = false;

        private Timer? _renderTimer;
        private readonly FpsCounter FpsCounter = new();

        public DisplayWindow(Profile profile)
        {
            // TODO: WinUI 3 equivalent for RenderOptions if needed
            // RenderOptions.ProcessRenderMode = RenderMode.Default;
            Profile = profile;
            DataContext = this;

            OpenGL = profile.OpenGL;

            this.InitializeComponent();
            InjectSkiaElement();

            if (profile.Resize)
            {
                ResizeMode = ResizeMode.CanResize;
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;
            }

            Loaded += Window_Loaded;
            Closing += DisplayWindow_Closing;

            Profile.PropertyChanged += Profile_PropertyChanged;
            ConfigModel.Instance.Settings.PropertyChanged += Config_PropertyChanged;

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            DpiChanged += DisplayWindow_DpiChanged;
            LocationChanged += DisplayWindow_LocationChanged;
            SizeChanged += DisplayWindow_SizeChanged;

            _dispatcherQueue = this.DispatcherQueue;
            _resizeTimer = _dispatcherQueue.CreateTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(300);
            _resizeTimer.Tick += OnResizeCompleted;
        }

        private void Window_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // WinUI 3 equivalent for window handle operations
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            //Performing some magic to hide the form from Alt+Tab
            _ = SetWindowLong(hwnd, GWL_EX_STYLE, (GetWindowLong(hwnd, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

            SetWindowPositionRelativeToScreen();

            UpdateSkiaTimer();

            Activate();
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

        private void DisplayWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Tick -= OnResizeCompleted;

            ConfigModel.Instance.Settings.PropertyChanged -= Config_PropertyChanged;

            if (_renderTimer != null)
            {
                _renderTimer.Stop();
                _renderTimer.Dispose();
                _renderTimer.Elapsed -= OnTimerElapsed;
            }

            Profile.PropertyChanged -= Profile_PropertyChanged;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

            DpiChanged -= DisplayWindow_DpiChanged;
            LocationChanged -= DisplayWindow_LocationChanged;
            SizeChanged -= DisplayWindow_SizeChanged;

            if (OpenGL)
            {
                foreach (var item in SharedModel.Instance.GetProfileDisplayItemsCopy(Profile))
                {
                    Clear(item);
                }
            }

            // TODO: Implement OpenGL cleanup when OpenGL support is added to WinUI 3 SkiaSharp
        }

        private void InjectSkiaElement()
        {
            var container = FindName("SkiaContainer") as Panel;
            if (container == null) return;

            container.Children.Clear();

            if (OpenGL)
            {
                AllowsTransparency = false;
                // TODO: Implement OpenGL equivalent for WinUI 3
                // SkiaSharp.Views.WinUI doesn't have SKGLElement yet
                // For now, use regular SKXamlCanvas as fallback
                var skElement = new SKXamlCanvas
                {
                    Name = "skElement",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                skElement.SetBinding(WidthProperty, new Binding { Path = new PropertyPath("Profile.Width"), Mode = BindingMode.OneWay });
                skElement.SetBinding(HeightProperty, new Binding { Path = new PropertyPath("Profile.Height"), Mode = BindingMode.OneWay });
                skElement.PaintSurface += SkElement_PaintSurface;
                container.Children.Add(skElement);

                _sKElement = skElement;
            }
            else
            {
                AllowsTransparency = true;
                var skElement = new SKXamlCanvas
                {
                    Name = "skElement",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                skElement.SetBinding(WidthProperty, new Binding { Path = new PropertyPath("Profile.Width"), Mode = BindingMode.OneWay });
                skElement.SetBinding(HeightProperty, new Binding { Path = new PropertyPath("Profile.Height"), Mode = BindingMode.OneWay });
                skElement.PaintSurface += SkElement_PaintSurface;
                container.Children.Add(skElement);

                _sKElement = skElement;
            }
        }

        private void DisplayWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Only track user-initiated size changes, not our DPI corrections
            if (!_isDpiChanging)
            {
                _isUserResizing = true;

                // Restart the timer - user is still resizing
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
        }

        // TODO: WinUI 3 doesn't have DpiChangedEventArgs - implement DPI handling differently
        private void DisplayWindow_DpiChanged(object sender, object e)
        {
            _isDpiChanging = true;
            MaintainPixelSize();
            _isDpiChanging = false;
        }

        private void DisplayWindow_LocationChanged(object? sender, EventArgs e)
        {
            _isDpiChanging = true;
            MaintainPixelSize();
            _isDpiChanging = false;
        }

        private void OnResizeCompleted(object? sender, EventArgs e)
        {
            _resizeTimer.Stop();

            if (_isUserResizing)
            {
                _isUserResizing = false;
                UpdateModelWithNewSize();
            }
        }

        private int _targetPixelWidth;
        private int _targetPixelHeight;

        private void MaintainPixelSize()
        {
            _targetPixelWidth = Profile.Width;
            _targetPixelHeight = Profile.Height;

            // TODO: WinUI 3 equivalent for DPI handling
            // WinUI 3 handles DPI differently than WPF
            // For now, use direct pixel values
            this.Width = _targetPixelWidth;
            this.Height = _targetPixelHeight;
        }

        private void UpdateModelWithNewSize()
        {
            // TODO: WinUI 3 equivalent for DPI conversion
            // For now, use direct pixel values
            Profile.Width = (int)Math.Round(this.ActualWidth);
            Profile.Height = (int)Math.Round(this.ActualHeight);
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _dispatcherQueue.TryEnqueue(() => _sKElement?.Invalidate());
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
                    if (Profile.Resize)
                    {
                        ResizeMode = ResizeMode.CanResize;
                    }
                    else
                    {
                        ResizeMode = ResizeMode.NoResize;
                    }
                });
            }
            else if (e.PropertyName == nameof(Profile.Width) || e.PropertyName == nameof(Profile.Height))
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    MaintainPixelSize();
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
                        case Microsoft.UI.Xaml.Input.VirtualKey.Up:
                            displayItem.Y -= SharedModel.Instance.MoveValue;
                            break;
                        case Microsoft.UI.Xaml.Input.VirtualKey.Down:
                            displayItem.Y += SharedModel.Instance.MoveValue;
                            break;
                        case Microsoft.UI.Xaml.Input.VirtualKey.Left:
                            displayItem.X -= SharedModel.Instance.MoveValue;
                            break;
                        case Microsoft.UI.Xaml.Input.VirtualKey.Right:
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
            }
            else if (this.IsVisible)
            {
                this.Hide();
            }
        }

        private void Window_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (!dragStart)
                {
                    var inSelectionBounds = false;
                    foreach (var displayItem in SharedModel.Instance.SelectedVisibleItems)
                    {
                        if (displayItem.ContainsPoint(e.GetPosition(this)))
                        {
                            inSelectionBounds = true;
                            break;
                        }
                    }

                    if (!inSelectionBounds)
                    {
                        foreach (var selectedItem in SharedModel.Instance.SelectedVisibleItems)
                        {
                            App.Current.Microsoft.UI.Xaml.Window.Current.DispatcherQueue.Invoke(() =>
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
                        this.DragMove();

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
                }
                else
                {
                    startPosition = e.GetPosition((UIElement)sender);

                    foreach (var item in SharedModel.Instance.SelectedVisibleItems)
                    {
                        item.MouseOffset = new Windows.Foundation.Point(startPosition.X - item.X, startPosition.Y - item.Y);
                    }

                    dragStart = true;
                }
            }
            else if (e.Key == Microsoft.UI.Xaml.Input.VirtualKey.Middle)
            {
                if (SharedModel.Instance.SelectedProfile != Profile)
                {
                    return;
                }

                DisplayItem? clickedItem = null;

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

                            if (groupItem.ContainsPoint(e.GetPosition(this)))
                            {
                                clickedItem = groupItem;
                                break;
                            }
                        }

                        if(clickedItem == null)
                        {
                            continue;
                        }
                    }

                    if (clickedItem != null)
                    {
                        break;
                    }

                    if (item.ContainsPoint(e.GetPosition(this)))
                    {
                        clickedItem = item;
                        break;
                    }
                }

                if (clickedItem != null)
                {

                    if (!Microsoft.UI.Input.KeyboardInput.GetKeyStateForCurrentThread(Microsoft.UI.VirtualKey.LeftShift).IsDown && !Microsoft.UI.Input.KeyboardInput.GetKeyStateForCurrentThread(Microsoft.UI.VirtualKey.LeftControl).IsDown)
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

        bool dragStart = false;
        Windows.Foundation.Point startPosition = new Windows.Foundation.Point();
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

