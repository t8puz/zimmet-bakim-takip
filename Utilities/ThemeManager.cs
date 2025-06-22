using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Utilities
{
    public enum AppTheme
    {
        Light,
        Dark
    }
    
    public class ThemeManager
    {
        // Tema değiştiğinde fırlatılacak olay
        public static event EventHandler<AppTheme>? ThemeChanged;
        
        // Mevcut tema
        private static AppTheme _currentTheme = AppTheme.Dark;
        
        // Tema değişimi devam ediyor mu?
        private static bool _isChangingTheme = false;
        
        // Mevcut temayı döndür
        public static AppTheme CurrentTheme => _currentTheme;
        
        // Tema kaynakları için Uri'lar
        private static readonly Uri _darkThemeUri = new Uri("/Zimmet_Bakim_Takip;component/Themes/DarkTheme.xaml", UriKind.Relative);
        private static readonly Uri _lightThemeUri = new Uri("/Zimmet_Bakim_Takip;component/Themes/LightTheme.xaml", UriKind.Relative);
                
        // Uygulama başlangıcında tema durumunu belirle
        static ThemeManager()
        {
            try
            {
                _currentTheme = AppTheme.Dark; // Varsayılan koyu tema
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ThemeManager başlatılırken hata: {ex.Message}");
            }
        }
        
        // Tema kaynakları ön-kontrol
        public static void InitializeThemes()
        {
            // Sadece ilk sefer tema dosyalarının var olduğunu kontrol et
            try
            {
                System.Diagnostics.Debug.WriteLine("Tema dosyaları kontrol ediliyor...");
                // Bir şey yapmaya gerek yok. Bu sadece herhangi bir hata varsa başlangıçta farketmek için
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tema dosyaları kontrol edilirken hata: {ex.Message}");
            }
        }
                
        // Tema değiştir - daha basit versiyon
        public static void ChangeTheme(AppTheme theme)
        {
            try 
            {
                // Tema değişim işlemi devam ediyorsa veya aynı tema seçilmişse, hiçbir şey yapma
                if (_isChangingTheme || _currentTheme == theme || Application.Current == null)
                    return;
                
                _isChangingTheme = true;
                
                // Tema değişimini zamanla
                Action action = () => {
                    try
                    {
                        // Tema değişimini uygula
                        _currentTheme = theme;
                        
                        // Seçilen temaya göre URI belirle
                        Uri themeUri = theme == AppTheme.Light ? _lightThemeUri : _darkThemeUri;
                        
                        // İlk önce tema kaynağını bulup çıkar
                        var mergedDicts = Application.Current.Resources.MergedDictionaries;
                        ResourceDictionary? themeDictToRemove = null;
                        
                        // Mevcut tema kaynaklarını bul
                        foreach (var dict in mergedDicts)
                        {
                            if (dict.Source != null && 
                                (dict.Source.ToString().Contains("DarkTheme.xaml") || 
                                 dict.Source.ToString().Contains("LightTheme.xaml")))
                            {
                                themeDictToRemove = dict;
                                break;
                            }
                        }
                        
                        // Temayı çıkar ve yenisini ekle
                        if (themeDictToRemove != null)
                        {
                            mergedDicts.Remove(themeDictToRemove);
                        }
                        
                        // Yeni tema sözlüğünü oluştur ve ekle
                        var newDict = new ResourceDictionary { Source = themeUri };
                        mergedDicts.Insert(0, newDict);
                        
                        // Tema değişim olayını bildir - UI thread üzerinde
                        Application.Current.Dispatcher.Invoke(() => {
                            ThemeChanged?.Invoke(null, theme);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Tema değişikliği sırasında UI thread'de hata: {ex.Message}");
                    }
                    finally
                    {
                        _isChangingTheme = false;
                    }
                };
                
                // Tema değişimini bir sonraki UI thread boşluğuna zamanla
                Application.Current.Dispatcher.BeginInvoke(action, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tema değişirken genel hata: {ex.Message}");
                _isChangingTheme = false;
            }
        }
        
        // Temayı değiştir (geçerli temanın tersine)
        public static void ToggleTheme()
        {
            ChangeTheme(_currentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        }
    }
} 