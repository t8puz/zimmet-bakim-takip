using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// String uzunluğuna göre görünürlük (Visibility) değeri döndüren dönüştürücü.
    /// Boş olmayan string değerleri için Visible, diğerleri için Collapsed değeri döndürür.
    /// </summary>
    public class StringLengthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && !string.IsNullOrEmpty(text))
                return Visibility.Visible;
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 