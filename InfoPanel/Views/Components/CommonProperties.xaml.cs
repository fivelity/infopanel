using InfoPanel.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel.Views.Components
{
    public sealed partial class CommonProperties : UserControl
    {
        public CommonProperties()
        {
            this.InitializeComponent();
            this.Loaded += CommonProperties_Loaded;
        }

        private void CommonProperties_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisibility();
            UpdateMoveValueText();

            SharedModel.Instance.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(SharedModel.IsSelectedItemsMovable))
                {
                    UpdateVisibility();
                }
                else if (args.PropertyName == nameof(SharedModel.SelectedItem))
                {
                    UpdatePositionValues();
                }
            };
        }

        private void UpdateVisibility()
        {
            GridMain.Visibility = SharedModel.Instance.IsSelectedItemsMovable ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdatePositionValues()
        {
            if (SharedModel.Instance.SelectedItem is DisplayItem item)
            {
                NumberBoxX.Value = item.X;
                NumberBoxY.Value = item.Y;
            }
        }

        private void UpdateMoveValueText()
        {
            TextBlockMoveValue.Text = $"{SharedModel.Instance.MoveValue} px";
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SharedModel.Instance.SelectedItems)
            {
                item.Y -= SharedModel.Instance.MoveValue;
            }
            UpdatePositionValues();
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SharedModel.Instance.SelectedItems)
            {
                item.Y += SharedModel.Instance.MoveValue;
            }
            UpdatePositionValues();
        }

        private void ButtonLeft_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SharedModel.Instance.SelectedItems)
            {
                item.X -= SharedModel.Instance.MoveValue;
            }
            UpdatePositionValues();
        }

        private void ButtonRight_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SharedModel.Instance.SelectedItems)
            {
                item.X += SharedModel.Instance.MoveValue;
            }
            UpdatePositionValues();
        }

        private void ButtonMoveValue_Click(object sender, RoutedEventArgs e)
        {
            SharedModel.Instance.MoveValue = SharedModel.Instance.MoveValue switch
            {
                1 => 5,
                5 => 10,
                10 => 20,
                20 => 1,
                _ => 1
            };
            UpdateMoveValueText();
        }

        private void NumberBoxX_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(args.NewValue) && SharedModel.Instance.SelectedItem is DisplayItem displayItem)
            {
                displayItem.X = (int)args.NewValue;
            }
        }

        private void NumberBoxY_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(args.NewValue) && SharedModel.Instance.SelectedItem is DisplayItem displayItem)
            {
                displayItem.Y = (int)args.NewValue;
            }
        }
    }
}
