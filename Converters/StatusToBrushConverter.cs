using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Converters
{
    /// <summary>
    /// Durum metnine göre fırça rengi döndürür
    /// </summary>
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
} 