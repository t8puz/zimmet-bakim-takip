using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// Boolean değeri fırça rengine dönüştürür (true=Yeşil, false=Kırmızı)
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 