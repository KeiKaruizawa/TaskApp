using System;
using System.Globalization;

namespace TaskApp.Converters
{
    public class ColorConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle null values
            if (value == null)
                return Colors.Gray;

            // Convert for Color type (category backgrounds and checkbox colors)
            if (targetType == typeof(Color))
            {
                var colorString = value.ToString();
                // Check if it's a valid hex color
                if (string.IsNullOrWhiteSpace(colorString) || !colorString.StartsWith("#"))
                    return Colors.Gray;

                try
                {
                    return Color.FromArgb(colorString);
                }
                catch
                {
                    return Colors.Gray;
                }
            }

            // Convert for Brush type (used in some XAML scenarios)
            if (targetType == typeof(Brush))
            {
                var colorString = value.ToString();
                if (string.IsNullOrWhiteSpace(colorString) || !colorString.StartsWith("#"))
                    return new SolidColorBrush(Colors.Gray);

                try
                {
                    return new SolidColorBrush(Color.FromArgb(colorString));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }

            // Convert for TextDecorations (strikethrough for completed tasks)
            if (targetType == typeof(TextDecorations))
            {
                if (value is bool isCompleted)
                {
                    return isCompleted ? TextDecorations.Strikethrough : TextDecorations.None;
                }
                return TextDecorations.None;
            }

            // Convert for progress bar visibility (show if there's progress)
            if (targetType == typeof(bool))
            {
                if (value is float percentage)
                {
                    return percentage > 0; // Show progress bar if percentage > 0
                }
                return false;
            }

            // Convert for progress bar width (percentage * 130) - INCREASED from 120 to 130
            if (targetType == typeof(double))
            {
                if (value is float percentage)
                {
                    return percentage * 130.0; // 130 is the full width (increased for longer bar)
                }
                return 0.0;
            }

            return value;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // For MultiBinding - calculate progress bar width
            if (values != null && values.Length > 0 && values[0] is float percentage)
            {
                return percentage * 130.0; // Full width is 130 (increased)
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}