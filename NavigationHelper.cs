using System.Windows;

namespace Zimmet_Bakim_Takip
{
    /// <summary>
    /// Navigasyon için yardımcı sınıf
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        /// "Page" adlı bir ek özellik (attached property) tanımı
        /// </summary>
        public static readonly DependencyProperty PageProperty =
            DependencyProperty.RegisterAttached(
                "Page",
                typeof(string),
                typeof(NavigationHelper),
                new PropertyMetadata(string.Empty));

        // Getter metodu
        public static string GetPage(DependencyObject obj)
        {
            return (string)obj.GetValue(PageProperty);
        }

        // Setter metodu
        public static void SetPage(DependencyObject obj, string value)
        {
            obj.SetValue(PageProperty, value);
        }
    }
} 