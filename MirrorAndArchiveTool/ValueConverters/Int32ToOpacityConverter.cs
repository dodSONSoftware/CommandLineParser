using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirrorAndArchiveTool.ValueConverters
{
    class Int32ToOpacityConverter
          : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((int)value == 0) ? 0.2 : 1.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
