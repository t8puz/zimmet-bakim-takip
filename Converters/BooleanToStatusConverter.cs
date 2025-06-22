using System;
using System.Globalization;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// Boolean değeri durum (status) metnine dönüştürür
    /// </summary>
    public class BooleanToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool tamamlandi)
            {
                return tamamlandi ? "Tamamlandı" : "Planlandı";
            }
            return "Bilinmiyor";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text == "Tamamlandı";
            }
            return false;
        }
    }
} 