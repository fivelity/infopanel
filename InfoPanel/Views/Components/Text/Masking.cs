using System;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InfoPanel
{
   

    /// <summary>
    /// Provides masking behavior for any <see cref="TextBox"/>.
    /// </summary>
    public static class Masking
    {
        private static readonly DependencyPropertyKey _maskExpressionPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MaskExpression",
                typeof(Regex),
                typeof(Masking),
                new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Mask"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaskProperty = DependencyProperty.RegisterAttached("Mask",
                typeof(string),
                typeof(Masking),
                new PropertyMetadata(null, OnMaskChanged));

        /// <summary>
        /// Identifies the <see cref="MaskExpression"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaskExpressionProperty = _maskExpressionPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the mask for a given <see cref="TextBox"/>.
        /// </summary>
        /// <param name="textBox">
        /// The <see cref="TextBox"/> whose mask is to be retrieved.
        /// </param>
        /// <returns>
        /// The mask, or <see langword="null"/> if no mask has been set.
        /// </returns>
        public static string GetMask(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(MaskProperty) as string;
        }

        /// <summary>
        /// Sets the mask for a given <see cref="TextBox"/>.
        /// </summary>
        /// <param name="textBox">
        /// The <see cref="TextBox"/> whose mask is to be set.
        /// </param>
        /// <param name="mask">
        /// The mask to set, or <see langword="null"/> to remove any existing mask from <paramref name="textBox"/>.
        /// </param>
        public static void SetMask(TextBox textBox, string mask)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            textBox.SetValue(MaskProperty, mask);
        }

        /// <summary>
        /// Gets the mask expression for the <see cref="TextBox"/>.
        /// </summary>
        /// <remarks>
        /// This method can be used to retrieve the actual <see cref="Regex"/> instance created as a result of setting the mask on a <see cref="TextBox"/>.
        /// </remarks>
        /// <param name="textBox">
        /// The <see cref="TextBox"/> whose mask expression is to be retrieved.
        /// </param>
        /// <returns>
        /// The mask expression as an instance of <see cref="Regex"/>, or <see langword="null"/> if no mask has been applied to <paramref name="textBox"/>.
        /// </returns>
        public static Regex GetMaskExpression(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("textBox");
            }

            return textBox.GetValue(MaskExpressionProperty) as Regex;
        }

        private static void SetMaskExpression(TextBox textBox, Regex regex)
        {
            textBox.SetValue(_maskExpressionPropertyKey, regex);
        }

        private static void OnMaskChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            var mask = e.NewValue as string;
            // Remove WinUI 3 event handlers
            textBox.TextChanged -= TextBox_TextChanged;
            textBox.KeyDown -= TextBox_KeyDown;
            textBox.Paste -= TextBox_Paste;
            textBox.Cutting -= TextBox_Cutting;
            textBox.Copying -= TextBox_Copying;

            if (mask == null)
            {
                textBox.ClearValue(MaskProperty);
                textBox.ClearValue(MaskExpressionProperty);
            }
            else
            {
                textBox.SetValue(MaskProperty, mask);
                SetMaskExpression(textBox, new Regex(mask, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace));
                // Add WinUI 3 event handlers
                textBox.TextChanged += TextBox_TextChanged;
                textBox.KeyDown += TextBox_KeyDown;
                textBox.Paste += TextBox_Paste;
                textBox.Cutting += TextBox_Cutting;
                textBox.Copying += TextBox_Copying;
            }
        }

        private static void TextBox_Cutting(object sender, TextControlCuttingEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            var maskExpression = GetMaskExpression(textBox);
            if (maskExpression == null) return;

            // Note: We can't easily prevent cutting in WinUI 3, so we'll validate after the cut
        }

        private static void TextBox_Copying(object sender, TextControlCopyingEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            
            // Allow copying regardless of mask
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            
            var maskExpression = GetMaskExpression(textBox);
            if (maskExpression == null) return;

            // Get the current text after the change
            var currentText = textBox.Text;
            if (!maskExpression.IsMatch(currentText))
            {
                // If the text doesn't match the mask, we need to remove the last character
                // This is a simplified approach - in a more robust implementation,
                // you would track which character was added
                if (currentText.Length > 0)
                {
                    var selectionStart = textBox.SelectionStart;
                    var beforeText = currentText.Substring(0, Math.Max(0, selectionStart - 1));
                    var afterText = currentText.Substring(Math.Min(currentText.Length, selectionStart));
                    textBox.Text = beforeText + afterText;
                    textBox.SelectionStart = beforeText.Length;
                }
            }
        }

        private static void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            
            var maskExpression = GetMaskExpression(textBox);
            if (maskExpression == null) return;

            // WinUI 3 doesn't have PreviewKeyDown, but we can catch specific keys
            // Since we're handling validation in TextBox_TextChanged, we don't need much here
            // In a more complete implementation, you could implement more specific key handling
        }

        private static void TextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            var maskExpression = GetMaskExpression(textBox);
            if (maskExpression == null) return;

            // WinUI 3 doesn't provide a way to inspect clipboard content before paste
            // Validation will happen in TextChanged event handler
        }

        // Note: Helper methods from WPF implementation are no longer needed with WinUI 3
        // implementation since validation is handled in TextBox_TextChanged event directly
    }
}
