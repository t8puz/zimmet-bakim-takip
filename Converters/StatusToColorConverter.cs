using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// Durum metnine göre renk değeri döndüren dönüştürücü.
    /// Farklı durum değerleri için farklı renk temaları kullanır.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.Gray);

            string status = value.ToString()?.ToLower() ?? string.Empty;

            switch (status)
            {
                case "müsait":
                case "available":
                case "aktif":
                case "tamamlandı":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Yeşil - #4CAF50
                
                case "zimmetli":
                case "assigned":
                case "gecikmeli":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Mavi - #2196F3
                
                case "bakımda":
                case "inmaintenance":
                case "beklemede":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Turuncu - #FF9800
                
                case "arızalı":
                case "damaged":
                case "iptal edildi":
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Kırmızı - #F44336
                
                case "emekli":
                case "retired":
                    return new SolidColorBrush(Colors.Gray);
                    
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 