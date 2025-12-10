using System;
using System.Globalization;
using System.Windows.Media;

namespace InfoPanel.Helpers
{
    public static class ColorHelper
    {
        public static Color ParseColor(string colorString, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return fallback;

            try
            {
                // Handle hex formats
                if (colorString.StartsWith("#"))
                {
                    // Remove #
                    var hex = colorString.Substring(1);

                    byte a = 255;
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;

                    if (hex.Length == 6)
                    {
                        // RRGGBB
                        r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                        g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                        b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                    }
                    else if (hex.Length == 8)
                    {
                        // AARRGGBB
                        a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                        r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                        g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                        b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                    }
                    else if (hex.Length == 3)
                    {
                        // RGB
                        r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
                        g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
                        b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);
                    }
                    else if (hex.Length == 4)
                    {
                        // ARGB
                        a = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
                        r = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
                        g = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);
                        b = byte.Parse(new string(hex[3], 2), NumberStyles.HexNumber);
                    }
                    else
                    {
                        return fallback;
                    }

                    return Color.FromArgb(a, r, g, b);
                }

                // Handle named colors
                if (ColorConverter.ConvertFromString(colorString) is Color color)
                {
                    return color;
                }
            }
            catch
            {
                // Fallback on error
            }

            return fallback;
        }
        
        public static SolidColorBrush GetBrush(string colorString, Color fallback)
        {
            var color = ParseColor(colorString, fallback);
            var brush = new SolidColorBrush(color);
            brush.Freeze(); // Freeze for performance and thread safety
            return brush;
        }
    }
}
