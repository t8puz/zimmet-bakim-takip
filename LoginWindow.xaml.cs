using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;

namespace Zimmet_Bakim_Takip
{
    public partial class LoginWindow : Window
    {
        // Statik kullanıcı nesnesi
        public static User? CurrentUser { get; set; }
        
        // Firma bilgisi sınıfı
        private CompanyInfo _companyInfo;
        
        // Firma bilgisi dosya yolu
        private readonly string _companyInfoPath;

        public LoginWindow()
        {
            InitializeComponent();
            
            // Başlangıçta kullanıcı null olmalı
            CurrentUser = null;
            
            // ENTER tuşu ile giriş yapma
            UsernameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptLogin(); };
            PasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptLogin(); };
            
            // Kayıt formunda Enter ile kayıt olma
            FirstNameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptRegister(); };
            LastNameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptRegister(); };
            EmailTextBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptRegister(); };
            RegisterUsernameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptRegister(); };
            RegisterPasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptRegister(); };
            ConfirmPasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Return) AttemptRegister(); };
            
            // Firma bilgileri sayfasında Enter ile kayıt
            CompanyNameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Return) SaveCompanyInfo(); };

            // Odağı kullanıcı adı alanına ver
            Loaded += (s, e) => UsernameTextBox.Focus();
            
            // Pencereyi sürüklemek için mouse down eventi
            MouseLeftButtonDown += (s, e) => 
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    DragMove();
            };
            
            // Dosya yolunu ayarla - uygulama veri klasörü
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
            
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _companyInfoPath = Path.Combine(appDataFolder, "company_info.json");
            
            // Firma bilgilerini yükle veya oluştur
            LoadCompanyInfo();
            
            // Son kaydedilen kullanıcı adını yükle
            string lastUsername = LoadLastLogin();
            if (!string.IsNullOrEmpty(lastUsername))
            {
                UsernameTextBox.Text = lastUsername;
                RememberLoginCheckBox.IsChecked = true; // Kullanıcı adı varsa, kutuyu da işaretleyelim
                // Şifre alanı boş bırakılmalı veya kullanıcıya şifreyi girmesi hatırlatılmalı
                // PasswordBox.Focus(); // Opsiyonel: Kullanıcı adı doluysa şifreye odaklan
            }

            // Varsayılan değerleri doldur (geliştirme kolaylığı için)
            #if DEBUG
            // Eğer son kullanıcı adı zaten yüklendiyse, DEBUG bloğundaki atamalar onu ezmemeli.
            // Bu yüzden bu bloğu LoadLastLogin'den sonraya aldık ve UsernameTextBox atamasını koşullu hale getirebiliriz.
            if (string.IsNullOrEmpty(UsernameTextBox.Text)) 
            {
                UsernameTextBox.Text = "admin";
            }
            // Şifre DEBUG modunda her zaman atanabilir, çünkü kaydedilmiyor.
            PasswordBox.Password = "admin123";
            #endif
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AttemptLogin();
        }

        private async Task AttemptLogin()
        {
            try
            {
                // UI kontrolleri devre dışı bırak
                SetLoginControlsEnabled(false);
                HideLoginError();

                // Giriş bilgilerini al
                string username = UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;

                // Boş kontrolü
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowLoginError("Lütfen kullanıcı adı ve şifre girin.");
                    return;
                }

                // Giriş işlemini dikkatli bir şekilde dene
                User? user = null;
                try
                {
                    // Kullanıcı servisini kontrol et
                    if (App.UserService == null)
                    {
                        ShowLoginError("Kullanıcı servisi başlatılamadı.");
                        return;
                    }

                    // Giriş kontrolü
                    user = await App.UserService.AuthenticateAsync(username, password);
                }
                catch (Exception authEx)
                {
                    ShowLoginError($"Kimlik doğrulama sırasında hata oluştu: {authEx.Message}", false);
                    System.Diagnostics.Debug.WriteLine($"Kimlik doğrulama hatası: {authEx.Message}");
                    return;
                }
                
                if (user == null)
                {
                    ShowLoginError("Kullanıcı adı veya şifre hatalı.");
                    return;
                }

                // Kullanıcı rolü kontrolü - sadece admin ve IT direktörü rollerine izin ver
                if (user.Role != "Admin" && user.Role != "IT_Director")
                {
                    ShowLoginError("Bu uygulamaya erişim yetkiniz bulunmamaktadır. Sadece Yöneticiler ve Admin kullanıcıları erişebilir.");
                    return;
                }

                // Beni hatırla
                if (RememberLoginCheckBox.IsChecked == true)
                {
                    // Hatırlama özelliği eklenebilir
                    SaveLastLogin(username);
                }

                // Giriş başarılı
                CurrentUser = user;
                
                // Ana pencereyi göster - bu içinde this.Close() çağrısı da var
                ShowMainWindow();
            }
            catch (Exception ex)
            {
                ShowLoginError($"Giriş yapılırken hata oluştu: {ex.Message}", false);
                System.Diagnostics.Debug.WriteLine($"Giriş hatası: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
            }
            finally
            {
                // UI kontrolleri tekrar etkinleştir
                SetLoginControlsEnabled(true);
            }
        }
        
        // Son başarılı girişi kaydet
        private void SaveLastLogin(string username)
        {
            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ZimmetBakimTakip",
                    "lastlogin.txt");
                    
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, username);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Son giriş kaydedilirken hata: {ex.Message}");
            }
        }
        
        // Son girişi yükle
        private string LoadLastLogin()
        {
            try
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ZimmetBakimTakip",
                    "lastlogin.txt");
                    
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Son giriş yüklenirken hata: {ex.Message}");
            }
            
            return string.Empty;
        }
        
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await AttemptRegister();
        }
        
        private async Task AttemptRegister()
        {
            try
            {
                // UI kontrolleri devre dışı bırak
                SetRegisterControlsEnabled(false);
                HideRegisterError();
                
                // Kayıt bilgilerini al
                string firstName = FirstNameTextBox.Text.Trim();
                string lastName = LastNameTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string username = RegisterUsernameTextBox.Text.Trim();
                string password = RegisterPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                
                // Boş alan kontrolü
                if (string.IsNullOrWhiteSpace(firstName) || 
                    string.IsNullOrWhiteSpace(lastName) || 
                    string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(username) || 
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ShowRegisterError("Lütfen tüm alanları doldurun.");
                    return;
                }
                
                // E-posta kontrolü
                if (!IsValidEmail(email))
                {
                    ShowRegisterError("Lütfen geçerli bir e-posta adresi girin.");
                    return;
                }
                
                // Şifre eşleşme kontrolü
                if (password != confirmPassword)
                {
                    ShowRegisterError("Şifreler eşleşmiyor.");
                    return;
                }
                
                // Şifre uzunluğu kontrolü
                if (password.Length < 6)
                {
                    ShowRegisterError("Şifre en az 6 karakter olmalıdır.");
                    return;
                }
                
                // Kullanıcı adı uzunluğu kontrolü
                if (username.Length < 3)
                {
                    ShowRegisterError("Kullanıcı adı en az 3 karakter olmalıdır.");
                    return;
                }
                
                // Kullanıcı var mı kontrolü
                var existingUser = await App.UserService.GetUserByUsernameAsync(username);
                if (existingUser != null)
                {
                    ShowRegisterError($"'{username}' kullanıcı adı zaten kullanılıyor.");
                    return;
                }
                
                // Yeni kullanıcı oluştur - IT direktörü rolüyle
                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username,
                    Email = email,
                    Role = "Admin", // Yönetici rolü
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                
                // Kullanıcıyı kaydet
                bool result = await App.UserService.AddUserAsync(newUser, password);
                
                if (result)
                {
                    MessageBox.Show("Hesap başarıyla oluşturuldu. Şimdi giriş yapabilirsiniz.", 
                                   "Kayıt Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Giriş formuna geç ve formu temizle
                    SwitchToLoginForm();
                    ClearRegisterForm();
                    
                    // Kullanıcı adını otomatik doldur
                    UsernameTextBox.Text = username;
                    PasswordBox.Focus();
                }
                else
                {
                    ShowRegisterError("Kullanıcı kaydedilirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                ShowRegisterError($"Kayıt sırasında hata oluştu: {ex.Message}");
            }
            finally
            {
                // UI kontrolleri tekrar etkinleştir
                SetRegisterControlsEnabled(true);
            }
        }
        
        // E-posta doğrulama
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ShowMainWindow()
        {
            try
            {
                // App referansını kontrol et
                var app = Application.Current as App;
                if (app == null)
                {
                    System.Diagnostics.Debug.WriteLine("App referansı alınamadı.");
                    MessageBox.Show("Uygulama başlatılamadı. Lütfen uygulamayı yeniden başlatın.", 
                                   "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
                
                // Services referansını kontrol et
                if (app.Services == null)
                {
                    System.Diagnostics.Debug.WriteLine("Services referansı alınamadı.");
                    MessageBox.Show("Servis sağlayıcı başlatılamadı. Lütfen uygulamayı yeniden başlatın.", 
                                   "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
                
                // Daha önceki ana pencere ve giriş pencerelerini kapatıldığına emin olalım
                foreach (Window window in Application.Current.Windows)
                {
                    if (window != this && window is MainWindow)
                    {
                        System.Diagnostics.Debug.WriteLine("Önceki MainWindow bulundu, kapatılıyor...");
                        window.Close();
                    }
                }
                
                // MainWindow nesnesini almayı dene
                try
                {
                    var mainWindow = app.Services.GetService(typeof(MainWindow)) as MainWindow;
                    if (mainWindow == null)
                    {
                        System.Diagnostics.Debug.WriteLine("MainWindow nesnesi oluşturulamadı.");
                        MessageBox.Show("Ana pencere oluşturulamadı. Lütfen uygulamayı yeniden başlatın.", 
                                       "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                        return;
                    }
                    
                    // Önce ana pencereyi göster
                    mainWindow.Show();
                    System.Diagnostics.Debug.WriteLine("Ana pencere başarıyla gösterildi.");
                    
                    // Sonra bu pencereyi kapat
                    this.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ana pencere alma hatası: {ex.Message}");
                    MessageBox.Show($"Ana pencere oluşturulurken hata oluştu: {ex.Message}. Lütfen uygulamayı yeniden başlatın.", 
                                   "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ana pencere gösterme hatası: {ex.Message}");
                MessageBox.Show($"Ana pencere açılırken hata oluştu: {ex.Message}. Lütfen uygulamayı yeniden başlatın.", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
        
        #region Form Geçişleri
        
        private void RegisterLink_Click(object sender, MouseButtonEventArgs e)
        {
            SwitchToRegisterForm();
        }
        
        private void LoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            SwitchToLoginForm();
        }
        
        private void SwitchToRegisterForm()
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            RegisterGrid.Visibility = Visibility.Visible;
            CompanySettingsGrid.Visibility = Visibility.Collapsed;
            FirstNameTextBox.Focus();
            HideLoginError();
        }
        
        private void SwitchToLoginForm()
        {
            RegisterGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
            CompanySettingsGrid.Visibility = Visibility.Collapsed;
            UsernameTextBox.Focus();
            HideRegisterError();
        }
        
        private void SwitchToCompanySettingsForm()
        {
            RegisterGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Collapsed;
            CompanySettingsGrid.Visibility = Visibility.Visible;
            CompanyNameTextBox.Focus();
        }
        
        #endregion
        
        #region Form Temizleme
        
        private void ClearLoginForm()
        {
            UsernameTextBox.Clear();
            PasswordBox.Clear();
            RememberLoginCheckBox.IsChecked = false;
            HideLoginError();
        }
        
        private void ClearRegisterForm()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            EmailTextBox.Clear();
            RegisterUsernameTextBox.Clear();
            RegisterPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            HideRegisterError();
        }
        
        #endregion
        
        #region Hata Mesajları
        
        private void ShowLoginError(string message, bool isError = true)
        {
            if (LoginErrorText != null)
            {
                LoginErrorText.Text = message;
                LoginErrorText.Foreground = isError 
                    ? System.Windows.Media.Brushes.Red 
                    : System.Windows.Media.Brushes.Blue;
                ErrorMessageBorder.Visibility = Visibility.Visible;
            }
        }

        private void HideLoginError()
        {
            LoginErrorText.Text = string.Empty;
            ErrorMessageBorder.Visibility = Visibility.Collapsed;
        }
        
        private void ShowRegisterError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
                
            RegisterErrorMessage.Text = message;
            RegisterErrorMessageBorder.Visibility = Visibility.Visible;
            System.Diagnostics.Debug.WriteLine($"Kayıt hatası: {message}");
            
            // Kayıt butonunu yeniden etkinleştir
            RegisterButton.IsEnabled = true;
        }

        private void HideRegisterError()
        {
            RegisterErrorMessage.Text = string.Empty;
            RegisterErrorMessageBorder.Visibility = Visibility.Collapsed;
        }
        
        private void ShowCompanyError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
                
            CompanyErrorMessage.Text = message;
            CompanyErrorMessageBorder.Visibility = Visibility.Visible;
        }
        
        private void HideCompanyError()
        {
            CompanyErrorMessage.Text = string.Empty;
            CompanyErrorMessageBorder.Visibility = Visibility.Collapsed;
        }
        
        #endregion
        
        #region UI Kontrol Durumları
        
        private void SetLoginControlsEnabled(bool enabled)
        {
            UsernameTextBox.IsEnabled = enabled;
            PasswordBox.IsEnabled = enabled;
            LoginButton.IsEnabled = enabled;
            RememberLoginCheckBox.IsEnabled = enabled;
            OfflineLoginButton.IsEnabled = enabled;
        }
        
        private void SetRegisterControlsEnabled(bool enabled)
        {
            FirstNameTextBox.IsEnabled = enabled;
            LastNameTextBox.IsEnabled = enabled;
            EmailTextBox.IsEnabled = enabled;
            RegisterUsernameTextBox.IsEnabled = enabled;
            RegisterPasswordBox.IsEnabled = enabled;
            ConfirmPasswordBox.IsEnabled = enabled;
            RegisterButton.IsEnabled = enabled;
        }
        
        private void SetCompanySettingsEnabled(bool enabled)
        {
            CompanyNameTextBox.IsEnabled = enabled;
            CompanyAddressTextBox.IsEnabled = enabled;
            CompanyPhoneTextBox.IsEnabled = enabled;
            TaxNumberTextBox.IsEnabled = enabled;
            TaxOfficeTextBox.IsEnabled = enabled;
            CompanyWebsiteTextBox.IsEnabled = enabled;
            SaveCompanyButton.IsEnabled = enabled;
            CancelCompanyButton.IsEnabled = enabled;
        }
        
        #endregion

        #region Firma Bilgileri İşlemleri
        
        // Firma bilgisi sınıfı
        private class CompanyInfo
        {
            public string CompanyName { get; set; } = string.Empty;
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
            public string TaxNumber { get; set; } = "";
            public string TaxOffice { get; set; } = "";
            public string Website { get; set; } = "";
            public DateTime LastUpdated { get; set; } = DateTime.Now;
        }
        
        // Firma bilgilerini yükle
        private void LoadCompanyInfo()
        {
            try
            {
                if (File.Exists(_companyInfoPath))
                {
                    string json = File.ReadAllText(_companyInfoPath);
                    _companyInfo = JsonSerializer.Deserialize<CompanyInfo>(json);
                    
                    // UI'ı güncelle
                    UpdateCompanyUI();
                }
                else
                {
                    // Varsayılan firma bilgisini oluştur
                    _companyInfo = new CompanyInfo();
                    
                    // UI'ı güncelle
                    UpdateCompanyUI();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firma bilgisi yüklenirken hata: {ex.Message}");
                _companyInfo = new CompanyInfo();
            }
        }
        
        // UI'ı firma bilgilerine göre güncelle
        private void UpdateCompanyUI()
        {
            // Sol paneldeki firma adı ve baş harfleri
            CompanyNameText.Text = string.IsNullOrEmpty(_companyInfo.CompanyName) ? "Şirket Adı" : _companyInfo.CompanyName;
            
            // Firma adını parçalayarak baş harfleri alma
            string initials = GetCompanyInitials(_companyInfo.CompanyName);
            CompanyInitialsText.Text = initials;
            
            // Şirket ayarları formunu güncelle
            if (CompanyNameTextBox != null)
            {
                if (!string.IsNullOrEmpty(_companyInfo.CompanyName))
                {
                    CompanyNameTextBox.Text = _companyInfo.CompanyName;
                }
                else
                {
                    CompanyNameTextBox.Text = string.Empty;
                }
                CompanyAddressTextBox.Text = _companyInfo.Address;
                CompanyPhoneTextBox.Text = _companyInfo.Phone;
                TaxNumberTextBox.Text = _companyInfo.TaxNumber;
                TaxOfficeTextBox.Text = _companyInfo.TaxOffice;
                CompanyWebsiteTextBox.Text = _companyInfo.Website;
            }
        }
        
        // Firma adından baş harfler oluştur
        private string GetCompanyInitials(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return "ŞA";
                
            var words = companyName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length == 0)
                return "ŞA";
                
            if (words.Length == 1)
            {
                // Tek kelimeyse ilk iki harfi al
                return companyName.Length > 1 
                    ? companyName.Substring(0, 2).ToUpper() 
                    : companyName.ToUpper() + "A";
            }
            
            // İlk iki kelimenin baş harflerini al
            return (words[0][0].ToString() + words[1][0].ToString()).ToUpper();
        }
        
        // Firma bilgilerini kaydet
        private void SaveCompanyInfo()
        {
            try
            {
                HideCompanyError();
                
                // Form verilerini doğrula
                string companyName = CompanyNameTextBox.Text.Trim();
                
                if (string.IsNullOrWhiteSpace(companyName))
                {
                    ShowCompanyError("Firma adı boş olamaz.");
                    CompanyNameTextBox.Focus();
                    return;
                }
                
                // Form verilerini al
                _companyInfo.CompanyName = companyName;
                _companyInfo.Address = CompanyAddressTextBox.Text.Trim();
                _companyInfo.Phone = CompanyPhoneTextBox.Text.Trim();
                _companyInfo.TaxNumber = TaxNumberTextBox.Text.Trim();
                _companyInfo.TaxOffice = TaxOfficeTextBox.Text.Trim();
                _companyInfo.Website = CompanyWebsiteTextBox.Text.Trim();
                _companyInfo.LastUpdated = DateTime.Now;
                
                // JSON olarak kaydet
                string json = JsonSerializer.Serialize(_companyInfo, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_companyInfoPath, json);
                
                // UI güncelle
                UpdateCompanyUI();
                
                // Login formuna geri dön
                SwitchToLoginForm();
                
                // Başarılı mesajı göster
                MessageBox.Show("Firma bilgileri başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowCompanyError($"Firma bilgileri kaydedilirken hata oluştu: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Firma bilgisi kaydedilirken hata: {ex.Message}");
            }
        }
        
        #endregion

        #region Buton Click Olayları
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Bu metod Google ile giriş özelliği kaldırıldığı için artık kullanılmıyor
        // Ancak referans problemleri yaşamamak için korundu
        // Butonun kendisi XAML dosyasından kaldırıldı
        private void GoogleLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Bu özellik kaldırıldı
        }
        
        // Firma Bilgileri Butonu
        private void CompanySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToCompanySettingsForm();
        }
        
        // Firma Bilgilerini Kaydet
        private void SaveCompanyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCompanyInfo();
        }
        
        // Firma Bilgilerini İptal Et
        private void CancelCompanyButton_Click(object sender, RoutedEventArgs e)
        {
            // Form verilerini yeniden yükle
            UpdateCompanyUI();
            
            // Login formuna geri dön
            SwitchToLoginForm();
        }
        
        // Çevrimdışı mod butonu
        private void OfflineLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Çevrimdışı mod için onay iste
            var result = MessageBox.Show(
                "Çevrimdışı modda çalışmak istediğinizden emin misiniz? Bu modda yapılan değişiklikler veritabanına kaydedilmeyebilir.",
                "Çevrimdışı Mod",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                // Çevrimdışı kullanıcı oluştur
                CurrentUser = new User
                {
                    Username = "offline_user",
                    FirstName = "Çevrimdışı",
                    LastName = "Kullanıcı",
                    Role = "Admin",
                    IsActive = true
                };
                
                // Ana pencereyi göster
                ShowMainWindow();
            }
        }
        
        // Şifremi unuttum butonu
        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(
                "Şifrenizi sıfırlamak için lütfen sistem yöneticinize başvurun.",
                "Şifre Sıfırlama",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        
        // Firma adı değiştiğinde baş harfleri güncelle
        private void CompanyNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string companyName = CompanyNameTextBox.Text.Trim();
            string initials = GetCompanyInitials(companyName);
            CompanyInitialsText.Text = initials;
        }
        #endregion
    }
} 