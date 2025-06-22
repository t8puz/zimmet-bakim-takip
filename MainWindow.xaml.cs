using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Zimmet_Bakim_Takip.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using Zimmet_Bakim_Takip.Pages;
using System.Text.Json;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using Zimmet_Bakim_Takip.Utilities;

namespace Zimmet_Bakim_Takip
{
    // CompanyInfo sınıfı - Firma bilgilerini tutmak için
    public class CompanyInfo
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Aşağıdaki özellikleri sol menüdeki kullanıcı ve firma bilgilerini güncellemek için kullanıyoruz
        public string UserFullName { get; set; } = "Kullanıcı";
        public string UserRole { get; set; } = "Roller";
        public string CompanyName { get; set; } = "Firma Adı";
        
        // Şu anki kullanıcının kimliği
        private int _currentUserId;
        
        // Şu anki tema modu
        private AppTheme _currentTheme = AppTheme.Dark;
        
        // Event, çıkış yapıldığında fires
        public event EventHandler LoggedOut;
        
        public MainWindow(int userId = 0)
        {
            try
            {
                InitializeComponent();
                
                // Pencere ikonu ayarla
                this.Icon = new BitmapImage(new Uri("pack://application:,,,/Images/logo.png", UriKind.Absolute));
                
                // Şu anki kullanıcı ID'sini ayarla
                _currentUserId = userId;
                
                // Şu anki kullanıcıya göre kullanıcı bilgilerini yükle
                LoadUserInfo();
                
                // Firma bilgilerini yükle
                LoadCompanyInfo();
                
                // İlk sayfayı yükle (Dashboard)
                MainFrame.Navigate(new DashboardPage());
                
                // Kullanıcı bilgilerini göster
                Loaded += MainWindow_Loaded;
                
                // Çıkış butonunun tooltip'ini değiştir
                var exitButton = FindName("ExitButton") as Button;
                if (exitButton != null)
                {
                    exitButton.Content = "Çıkış";
                    exitButton.ToolTip = "Uygulamayı kapat";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow constructor hatası: {ex.Message}");
                MessageBox.Show($"Ana pencere başlatılırken hata oluştu: {ex.Message}", 
                               "Başlatma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Varsayılan kullanıcı bilgileri göster
                UserFullNameText.Text = UserFullName;
                UserRoleText.Text = UserRole;
                UserInitials.Text = GetUserInitials(UserFullName);
                
                // Firma bilgilerini yükle ve göster
                LoadCompanyInfo();
                
                // Pencere başlığını ve firma bilgilerini güncelle
                if (!string.IsNullOrEmpty(CompanyName))
                {
                    this.Title = $"{CompanyName} - IT Envanter Yönetim Sistemi";
                    
                    // Sol menüdeki firma adını göster
                    CompanyNameText.Text = CompanyName;
                    
                    // Firma baş harfleri UI elemanı artık kullanılmıyor, yorum satırı yapıldı
                    // CompanyInitials.Text = GetCompanyInitials(CompanyName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcı bilgileri yüklenirken hata: {ex.Message}", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCompanyInfo()
        {
            try
            {
                // Firma bilgileri yolunu al
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
                string companyInfoPath = Path.Combine(appDataFolder, "company_info.json");
                
                if (File.Exists(companyInfoPath))
                {
                    // JSON dosyasını oku
                    string json = File.ReadAllText(companyInfoPath);
                    
                    // JSON dosyasını Company nesnelerine dönüştür
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var companyInfo = JsonSerializer.Deserialize<CompanyInfo>(json);
                        
                        if (companyInfo != null)
                        {
                            // Firma adını ayarla
                            CompanyName = companyInfo.CompanyName;
                            
                            // UI güncelle
                            CompanyNameText.Text = companyInfo.CompanyName;
                            
                            // Başlığı güncelle
                            Title = $"{companyInfo.CompanyName} - IT Envanter Yönetim Sistemi";
                            
                            // Firma baş harfleri UI elemanı artık kullanılmıyor, yorum satırı yapıldı
                            // CompanyInitials.Text = GetCompanyInitials(companyInfo.CompanyName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Firma bilgileri yüklenirken hata: {ex.Message}");
                
                // Varsayılan isim
                CompanyName = "Firma Adı";
                CompanyNameText.Text = CompanyName;
                // Firma baş harfleri UI elemanı artık kullanılmıyor, yorum satırı yapıldı
                // CompanyInitials.Text = "FA";
            }
        }
        
        private void LoadUserInfo()
        {
            try
            {
                // Varsayılan kullanıcı bilgileri
                UserFullName = "Sistem Kullanıcısı";
                UserRole = "Yönetici";
                
                // Kullanıcı bilgilerini UI'da göster
                UserFullNameText.Text = UserFullName;
                UserRoleText.Text = UserRole;
                UserInitials.Text = GetUserInitials(UserFullName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kullanıcı bilgileri yüklenirken hata: {ex.Message}");
            }
        }

        private string GetUserInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "SK"; // Sistem Kullanıcısı
            
            var words = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length >= 2)
            {
                return $"{words[0][0]}{words[1][0]}".ToUpper();
            }
            
            // Tek kelimeyse ilk iki harfi al ya da tek harf ve K harfi
            return fullName.Length > 1
                ? fullName.Substring(0, 2).ToUpper()
                : fullName.ToUpper() + "K";
        }

        private string GetCompanyInitials(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return "ZT"; // Varsayılan
            
            var words = companyName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length >= 2)
            {
                return $"{words[0][0]}{words[1][0]}".ToUpper();
            }
            
            // Tek kelimeyse ilk iki harfi al ya da tek harf ve T harfi
            return companyName.Length > 1
                ? companyName.Substring(0, 2).ToUpper()
                : companyName.ToUpper() + "T";
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button)
            {
                string pageName = NavigationHelper.GetPage(button);
                if (string.IsNullOrEmpty(pageName)) return;
                
                // Sayfaya gitme işlemi
                NavigateToPage(pageName);
            }
        }

        /// <summary>
        /// Visual ağaçta belirli bir tipte tüm child elementleri bulmak için yardımcı metod
        /// </summary>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject? child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T t)
                {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        private void NavigateToPage(string pageName)
        {
            if (string.IsNullOrEmpty(pageName)) return;
            
            // Sayfa nesnesini oluştur
            object? page = null;

            try 
            {
                switch (pageName)
                {
                    case "Dashboard":
                        page = new DashboardPage();
                        break;
                    case "Devices":
                        page = new DevicesPage();
                        break;
                    case "Personnel":
                        page = new PersonnelPage();
                        break;
                    case "Assignments":
                        page = new AssignmentsPage();
                        break;
                    case "Maintenance":
                        page = new MaintenanceRecordPage();
                        break;
                    case "Settings":
                        page = new SettingsPage();
                        break;
                    case "Profile":
                        page = new ProfilePage();
                        break;
                    default:
                        MessageBox.Show($"Sayfa bulunamadı: {pageName}", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }

                // Sayfa oluşturulduysa içeriğini göster
                if (page != null)
                {
                    // Animasyon ile sayfayı değiştir
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.3)
                    };

                    MainFrame.Opacity = 0;
                    MainFrame.Navigate(page);
                    MainFrame.BeginAnimation(OpacityProperty, animation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sayfa yüklenirken hata oluştu: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Doğrudan uygulamayı kapat
            Application.Current.Shutdown();
        }

        // Pencere kontrol butonları için olay işleyicileri
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                
                // Maximize/Restore ikonunu güncelle (Restore simgesinden Maximize simgesine)
                var textBlock = ((Button)sender).Content as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = "\uE739"; // Maximize ikonu
                }
            }
            else
            {
                WindowState = WindowState.Maximized;
                
                // Maximize/Restore ikonunu güncelle (Maximize simgesinden Restore simgesine)
                var textBlock = ((Button)sender).Content as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = "\uE923"; // Restore ikonu
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Pencere sürükleme işlemleri
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            
            // Sadece üst barda tıklandığında sürükleme işlemini etkinleştir
            if (e.GetPosition(this).Y < 50)
            {
                this.DragMove();
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void Assets_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Assets page
            MessageBox.Show("Zimmet sayfası yakında eklenecek.");
        }

        private void Maintenance_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Maintenance page
            MessageBox.Show("Bakım sayfası yakında eklenecek.");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}