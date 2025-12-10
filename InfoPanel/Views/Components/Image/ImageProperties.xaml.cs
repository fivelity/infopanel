using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;

namespace InfoPanel.Views.Components
{
    public sealed partial class ImageProperties : UserControl
    {
        public ImageProperties()
        {
            this.InitializeComponent();
            ComboBoxType.ItemsSource = Enum.GetValues(typeof(ImageDisplayItem.ImageType)).Cast<ImageDisplayItem.ImageType>();
        }

        private async void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is ImageDisplayItem imageDisplayItem)
            {
                var fileOpenPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.ComputerFolder
                };
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.FileTypeFilter.Add(".jpeg");
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.FileTypeFilter.Add(".svg");
                fileOpenPicker.FileTypeFilter.Add(".gif");
                fileOpenPicker.FileTypeFilter.Add(".webp");
                fileOpenPicker.FileTypeFilter.Add(".mp4");
                fileOpenPicker.FileTypeFilter.Add(".mkv");
                fileOpenPicker.FileTypeFilter.Add(".webm");
                fileOpenPicker.FileTypeFilter.Add(".avi");
                fileOpenPicker.FileTypeFilter.Add(".mov");

                var file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    var profile = SharedModel.Instance.SelectedProfile;

                    if (profile != null)
                    {
                        var imageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InfoPanel", "assets", profile.Guid.ToString());
                        if (!Directory.Exists(imageFolder))
                        {
                            Directory.CreateDirectory(imageFolder);
                        }

                        try
                        {
                            var copy = (ImageDisplayItem) imageDisplayItem.Clone();

                            var fileName = openFileDialog.SafeFileName;

                            var filePath = Path.Combine(imageFolder, openFileDialog.SafeFileName);
                            File.Copy(openFileDialog.FileName, filePath, true);

                            imageDisplayItem.Guid = Guid.NewGuid();
                            imageDisplayItem.RelativePath = true;
                            imageDisplayItem.Name = openFileDialog.SafeFileName;
                            imageDisplayItem.FilePath = fileName;

                            Cache.InvalidateImage(copy);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private void CheckBoxCache_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SharedModel.Instance.SelectedItem is ImageDisplayItem imageDisplayItem && !imageDisplayItem.Cache
                && Cache.GetLocalImage(imageDisplayItem) is LockedImage lockedImage)
            {
                lockedImage.DisposeSKAssets();
                lockedImage.DisposeGLAssets();
            }
        }
    }
}
