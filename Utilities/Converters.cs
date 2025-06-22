using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Zimmet_Bakim_Takip.Utilities
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
            throw new NotImplementedException();
        }
    }

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

    // Zimmet durumuna göre renk döndüren converter
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Application.Current.Resources["SidebarBackgroundBrush"];

            string status = value.ToString()?.ToLower() ?? string.Empty;
            
            switch (status)
            {
                case "müsait":
                case "available":
                case "aktif":
                case "tamamlandı":
                    return Application.Current.Resources["SuccessBrush"];
                
                case "zimmetli":
                case "assigned":
                case "gecikmeli":
                    return Application.Current.Resources["PrimaryBrush"];
                
                case "bakımda":
                case "inmaintenance":
                case "beklemede":
                    return Application.Current.Resources["WarningBrush"];
                
                case "arızalı":
                case "damaged":
                case "iptal edildi":
                    return Application.Current.Resources["ErrorBrush"];
                
                default:
                    return Application.Current.Resources["SidebarBackgroundBrush"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// String boşsa Visible, değilse Collapsed döndüren converter.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                // Parametre true ise tersini döndür (yani boşsa collapsed, değilse visible)
                bool reverse = parameter is bool b && b;
                
                bool isEmpty = string.IsNullOrEmpty(text);
                
                if (reverse)
                {
                    return isEmpty ? Visibility.Collapsed : Visibility.Visible;
                }
                
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToActiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Aktif" : "Pasif";
            }
            return "Bilinmiyor";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() == "aktif";
            }
            return false;
        }
    }
    
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