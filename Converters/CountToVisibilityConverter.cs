using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// Bir koleksiyondaki öğe sayısını kontrol ederek görünürlük değeri döndürür
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Kontrol edilecek sayı (varsayılan 0)
            int compareValue = 0;
            
            // Parameter değeri varsa kullan
            if (parameter != null)
            {
                if (int.TryParse(parameter.ToString(), out int parsedValue))
                {
                    compareValue = parsedValue;
                }
            }
            
            // Eğer value null ise veya sayımı 0 ise Visible, değilse Collapsed
            if (value == null)
            {
                return Visibility.Visible;
            }
            
            // ICollection ise Count özelliğini kullan
            if (value is ICollection collection)
            {
                return collection.Count == compareValue ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Int sayı ise direkt karşılaştır
            if (value is int count)
            {
                return count == compareValue ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 