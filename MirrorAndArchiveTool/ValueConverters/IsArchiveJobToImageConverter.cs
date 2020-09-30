using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool.ValueConverters
{
    public class IsArchiveJobToImageConverter
                : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value)
                ? "pack://application:,,,/MirrorAndArchiveTool;component//Images/Actions-dialog-ok-apply.ico"
                : "pack://application:,,,/MirrorAndArchiveTool;component//Images/Status-dialog-error.ico";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
