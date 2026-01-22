using System;
using System.Globalization;

namespace TaskApp.Converters
{
    public class ProgressColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.Gray;

            var colorString = value.ToString();

            // Check if it's a valid hex color
            if (string.IsNullOrWhiteSpace(colorString) || !colorString.StartsWith("#"))
                return Colors.Gray;

            try
            {
                // Get the original color
                var originalColor = Color.FromArgb(colorString);

                // Darken it for the progress bar (40% darker)
                return Color.FromRgba(
                    originalColor.Red * 0.6,
                    originalColor.Green * 0.6,
                    originalColor.Blue * 0.6,
                    originalColor.Alpha
                );
            }
            catch
            {
                return Colors.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}