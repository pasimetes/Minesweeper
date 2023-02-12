using System;
using System.Globalization;
using System.Windows.Data;

namespace Minesweeper.Converters
{
    public class TimespanStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timespan = TimeSpan.FromSeconds((int)value);
            var timeString = string.Format(
                "{0:D2}:{1:D2}",
                timespan.Minutes,
                timespan.Seconds);

            return timeString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
