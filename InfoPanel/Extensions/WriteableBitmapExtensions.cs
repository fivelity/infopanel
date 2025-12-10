using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace InfoPanel.Extensions
{
    public static class WriteableBitmapExtensions
    {
        public static async Task<BitmapImage> ToBitmapImageAsync(this SoftwareBitmap wbm)
        {
            if (wbm == null) return null;
            
            var bitmapImage = new BitmapImage();
            
            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetSoftwareBitmap(wbm);
                await encoder.FlushAsync();
                
                await bitmapImage.SetSourceAsync(stream);
            }
            
            return bitmapImage;
        }

        public static void UpdateFrom(this WriteableBitmap target, WriteableBitmap source)
        {
            // WinUI 3 WriteableBitmap doesn't have the same API as WPF
            // This method would need to be reimplemented based on specific requirements
            // For now, placeholder implementation
            throw new NotImplementedException("WriteableBitmap.UpdateFrom needs WinUI 3 implementation");
        }
    }
}
