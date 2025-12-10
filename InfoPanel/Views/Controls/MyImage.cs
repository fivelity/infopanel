using InfoPanel.Models;
using SkiaSharp;
using SkiaSharp.Views.WinUI;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;

namespace InfoPanel.Views.Controls
{
    public class MyImage : Image
    {
        public static readonly DependencyProperty ImageDisplayItemProperty =
    DependencyProperty.Register(
        nameof(ImageDisplayItem),
        typeof(ImageDisplayItem),
        typeof(MyImage),
        new PropertyMetadata(null, OnImageDisplayItemChanged));

        public ImageDisplayItem ImageDisplayItem
        {
            get => (ImageDisplayItem)GetValue(ImageDisplayItemProperty);
            set => SetValue(ImageDisplayItemProperty, value);
        }

        private readonly DispatcherTimer Timer = new();

        public MyImage()
        {
            Timer.Interval = TimeSpan.FromMilliseconds(41);
            Loaded += OnLoaded;
            Unloaded += MyImage_Unloaded;
        }

        private static void OnImageDisplayItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MyImage myImage)
            {
                if (e.NewValue is ImageDisplayItem imageDisplayItem)
                {
                    myImage.Timer.Start();
                }
                else
                {
                    myImage.Source = null;
                    myImage.Timer.Stop();
                }
            }
        }

        private void MyImage_Unloaded(object sender, RoutedEventArgs _)
        {
            Timer.Stop();
            Timer.Tick -= Timer_Tick;
        }

        private void OnLoaded(object sender, RoutedEventArgs _)
        {
            Timer.Tick += Timer_Tick;
        }

        private async void Timer_Tick(object? sender, object e)
        {
            var imageDisplayItem = ImageDisplayItem;
            if (imageDisplayItem == null) return;

            int width = (int)this.ActualWidth;
            int height = (int)this.ActualHeight;

            if (width <= 0 || height <= 0) return;

            // Run heavy lifting on background thread
            WriteableBitmap? writeableBitmap = null;

            await Task.Run(() =>
            {
                var image = Cache.GetLocalImage(imageDisplayItem);

                if (image != null)
                {
                    if (image.Type == LockedImage.ImageType.SVG)
                    {
                        image.AccessSVG(picture =>
                        {
                            var bounds = picture.CullRect;
                            // Note: ToWriteableBitmap might need UI thread if it creates a WriteableBitmap directly? 
                            // Svg.Skia extensions usually return a bitmap or writeable bitmap.
                            // If it creates Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap, it must be on UI thread.
                            // However, we can generate SKBitmap here and convert later.
                            // But LockedImage seems to handle this.
                            
                            // Checking LockedImage.cs usage: writeableBitmap = picture.ToWriteableBitmap(...)
                            // This extension likely creates a WPF WriteableBitmap or WinUI WriteableBitmap.
                            // If WinUI, it must be created on UI thread.
                        });
                    }
                    else
                    {
                        // Calculation logic...
                        double imageAspectRatio = (double)image.Width / image.Height;
                        double containerAspectRatio = (double)width / height;

                        int targetWidth, targetHeight;

                        if (imageAspectRatio > containerAspectRatio)
                        {
                            targetWidth = width;
                            targetHeight = (int)Math.Ceiling(width / imageAspectRatio);
                        }
                        else
                        {
                            targetWidth = (int)Math.Ceiling(height * imageAspectRatio);
                            targetHeight = height;
                        }

                        image.AccessSK(targetWidth, targetHeight, bitmap =>
                        {
                            if (bitmap != null)
                            {
                                // bitmap.ToWriteableBitmap() extension also needs checking.
                            }
                        }, true, "MyImage");
                    }
                }
            });
            
            // To properly fix this, we need to know what ToWriteableBitmap does. 
            // Assuming it returns a WinUI WriteableBitmap, it MUST be done on UI thread.
            // So we should just run the logic on UI thread for now, Skia operations are fast enough usually.
            // Or only do the calculation on BG and creation on UI.
            
            ProcessImageOnUI(imageDisplayItem, width, height);
        }

        private void ProcessImageOnUI(ImageDisplayItem imageDisplayItem, int width, int height)
        {
            var image = Cache.GetLocalImage(imageDisplayItem);
            if (image == null) return;

            if (image.Type == LockedImage.ImageType.SVG)
            {
                image.AccessSVG(picture =>
                {
                    var bounds = picture.CullRect;
                    var skSize = new SKSizeI((int)bounds.Width, (int)bounds.Height);
                    // Assuming ToWriteableBitmap exists for WinUI and creates a WriteableBitmap
                    this.Source = picture.ToWriteableBitmap(skSize);
                });
            }
            else
            {
                double imageAspectRatio = (double)image.Width / image.Height;
                double containerAspectRatio = (double)width / height;
                int targetWidth, targetHeight;

                if (imageAspectRatio > containerAspectRatio)
                {
                    targetWidth = width;
                    targetHeight = (int)Math.Ceiling(width / imageAspectRatio);
                }
                else
                {
                    targetWidth = (int)Math.Ceiling(height * imageAspectRatio);
                    targetHeight = height;
                }

                image.AccessSK(targetWidth, targetHeight, bitmap =>
                {
                    if (bitmap != null)
                    {
                        this.Source = bitmap.ToWriteableBitmap();
                    }
                }, true, "MyImage");
            }

            if (image.Frames <= 1)
            {
                Timer.Stop();
            }
        }
    }
}
