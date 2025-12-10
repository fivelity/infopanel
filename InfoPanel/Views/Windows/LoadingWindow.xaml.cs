using Microsoft.UI.Xaml;

namespace InfoPanel.Views.Windows
{
    public sealed partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            this.InitializeComponent();
        }

        public void SetText(string text)
        {
            TextBlockStatus.Text = text;
        }
    }
}
