using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool.ValueConverters
{
    public class DateLastRanToDisplayStringConverter
        : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var date = (DateTime)value;
            if (date == DateTime.MinValue) { return "Never Ran"; }
            var diff = (DateTime.Now - date);
            if (diff < TimeSpan.FromMinutes(1))
            {
                return "less than a minute ago.";
            }
            else if (diff < TimeSpan.FromHours(1))
            {
                return string.Format("{0:N0} minutes ago.", diff.TotalMinutes);
            }
            return date.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
