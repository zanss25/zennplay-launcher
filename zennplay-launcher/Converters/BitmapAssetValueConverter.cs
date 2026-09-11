using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Converters
{
    public class BitmapAssetValueConverter : IValueConverter
    {
        public static readonly BitmapAssetValueConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string rawPath && !string.IsNullOrWhiteSpace(rawPath) && File.Exists(rawPath))
            {
                try
                {
                    return new Bitmap(rawPath);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
