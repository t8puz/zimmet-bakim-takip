using System;
using System.Globalization;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// String'in boş olup olmadığını kontrol eden converter.
    /// String boşsa true, değilse false döndürür.
    /// </summary>
    public class StringIsEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return string.IsNullOrEmpty(text);
            }
            return true; // Varsayılan olarak boş
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 