using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Zimmet_Bakim_Takip.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using System.Windows.Media;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// ProfilePage.xaml etkileşim mantığı
    /// </summary>
    public partial class ProfilePage : Page
    {
        private User? _currentUser;
        private CompanyInfo _companyInfo = new CompanyInfo();
        
        public ProfilePage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kullanıcı bilgilerini yükle
                LoadUserData();
                
                // Firma bilgilerini yükle
                LoadCompanyData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Profil bilgileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadUserData()
        {
            try
            {
                // Varsayılan sistem kullanıcısı bilgilerini yükle
                FirstNameTextBox.Text = "Sistem";
                LastNameTextBox.Text = "Kullanıcısı";
                UsernameTextBox.Text = "admin";
            
                // Kullanıcı adının baş harflerini al
                string initials = GetUserInitials("Sistem", "Kullanıcısı");
                UserInitialsText.Text = initials;
            
                // Rol bilgisi
                UserRoleText.Text = "Yönetici";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcı bilgileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadCompanyData()
        {
            try
            {
                // Firma bilgilerini dosyadan yükle
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
                string companyInfoPath = Path.Combine(appDataFolder, "company_info.json");
                
                // Dizin yoksa oluştur
                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }
                
                if (File.Exists(companyInfoPath))
                {
                    string json = File.ReadAllText(companyInfoPath);
                    
                    // JSON yoksa veya içi boşsa varsayılan değerleri kullan
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        _companyInfo = JsonSerializer.Deserialize<CompanyInfo>(json) ?? new CompanyInfo();
                    }
                }
                
                // Firma adı ve bilgilerini doldur
                CompanyNameTextBox.Text = _companyInfo.CompanyName;
                CompanyAddressTextBox.Text = _companyInfo.Address;
                CompanyContactTextBox.Text = _companyInfo.Contact;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firma bilgileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string GetUserInitials(string firstName, string lastName)
        {
            string initials = "";
            
            if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
            {
                initials += firstName[0];
            }
            
            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
            {
                initials += lastName[0];
            }
            
            return initials.ToUpper();
        }
        
        private string GetCompanyInitials(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return "ŞA"; // Şirket Adı
            
            var words = companyName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length >= 2)
            {
                return $"{words[0][0]}{words[1][0]}".ToUpper();
            }
            
            // Tek kelimeyse ilk iki harfi al ya da tek harf ve A harfi
            return companyName.Length > 1
                ? companyName.Substring(0, 2).ToUpper()
                : companyName.ToUpper() + "A";
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Formdaki değerleri al
                string firstName = FirstNameTextBox.Text?.Trim() ?? "";
                string lastName = LastNameTextBox.Text?.Trim() ?? "";
                
                // Boşluk kontrolü
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    ShowError("İsim ve soyisim alanları boş bırakılamaz!");
                    return;
                }
                
                // Kullanıcı bilgilerini güncelle (varsayılan kullanıcı için sadece UI güncellenir)
                MessageBox.Show("Kullanıcı bilgileri güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Ekrandaki bilgileri güncelle (baş harfler vb.)
                string initials = GetUserInitials(firstName, lastName);
                UserInitialsText.Text = initials;
                
                // Ana penceredeki kullanıcı bilgilerini güncelle 
                UpdateMainWindowUserInfo(firstName, lastName, initials);
                
                // Hata mesajını temizle
                HideError();
            }
            catch (Exception ex)
            {
                ShowError($"Kullanıcı bilgileri kaydedilirken hata oluştu: {ex.Message}");
            }
        }
        
        private void SaveCompanyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Formdaki değerleri al
                string companyName = CompanyNameTextBox.Text?.Trim() ?? "";
                string address = CompanyAddressTextBox.Text?.Trim() ?? "";
                string contact = CompanyContactTextBox.Text?.Trim() ?? "";
                
                // Boşluk kontrolü
                if (string.IsNullOrWhiteSpace(companyName))
                {
                    ShowCompanyError("Firma adı boş bırakılamaz!");
                    return;
                }
                
                // Firma bilgilerini güncelle
                _companyInfo.CompanyName = companyName;
                _companyInfo.Address = address;
                _companyInfo.Contact = contact;
                
                // Firma bilgilerini dosyaya kaydet
                SaveCompanyInfo();
                
                // Ana penceredeki firma adını güncelle
                UpdateMainWindowCompanyInfo();
                
                MessageBox.Show("Firma bilgileri başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Hata mesajını temizle
                HideCompanyError();
            }
            catch (Exception ex)
            {
                ShowCompanyError($"Firma bilgileri kaydedilirken hata oluştu: {ex.Message}");
            }
        }
        
        private void SaveCompanyInfo()
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
                string companyInfoPath = Path.Combine(appDataFolder, "company_info.json");
                
                // Dizin yoksa oluştur
                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }
                
                // CompanyInfo nesnesini JSON'a dönüştür
                string json = JsonSerializer.Serialize(_companyInfo, new JsonSerializerOptions { WriteIndented = true });
                
                // Dosyaya yaz
                File.WriteAllText(companyInfoPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception("Firma bilgileri kaydedilemedi: " + ex.Message);
            }
        }
        
        private void UpdateMainWindowUserInfo(string firstName, string lastName, string initials)
        {
            try
            {
                // Ana pencereyi bul
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // Kullanıcı bilgilerini güncelle
                    TextBlock? userFullNameText = mainWindow.FindName("UserFullNameText") as TextBlock;
                    TextBlock? userInitials = mainWindow.FindName("UserInitials") as TextBlock;
                    
                    if (userFullNameText != null)
                    {
                        userFullNameText.Text = $"{firstName} {lastName}";
                    }
                    
                    if (userInitials != null)
                    {
                        userInitials.Text = initials;
                    }
                }
            }
            catch (Exception ex)
            {
                // Güncelleme hatası kritik değil, sessizce geç
                System.Diagnostics.Debug.WriteLine($"Ana pencere kullanıcı bilgileri güncellenirken hata: {ex.Message}");
            }
        }
        
        private void UpdateMainWindowCompanyInfo()
        {
            try
            {
                // Ana pencereyi bul
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // Pencere başlığını güncelle
                    if (!string.IsNullOrEmpty(_companyInfo.CompanyName))
                    {
                        mainWindow.Title = $"{_companyInfo.CompanyName} - IT Envanter Yönetim Sistemi";
                        
                        // CompanyName özelliğini güncelle
                        mainWindow.CompanyName = _companyInfo.CompanyName;
                    }
                    
                    // Firma adını güncelle
                    TextBlock? companyNameText = mainWindow.FindName("CompanyNameText") as TextBlock;
                    TextBlock? companyInitials = mainWindow.FindName("CompanyInitials") as TextBlock;
                    
                    if (companyNameText != null)
                    {
                        companyNameText.Text = _companyInfo.CompanyName;
                    }
                    
                    if (companyInitials != null)
                    {
                        companyInitials.Text = GetCompanyInitials(_companyInfo.CompanyName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Güncelleme hatası kritik değil, sessizce geç
                System.Diagnostics.Debug.WriteLine($"Ana pencere firma bilgileri güncellenirken hata: {ex.Message}");
            }
        }
        
        private void UserTabButton_Checked(object sender, RoutedEventArgs e)
        {
            if (UserProfilePanel == null || CompanyProfilePanel == null || PasswordChangePanel == null)
                return;
                
            UserProfilePanel.Visibility = Visibility.Visible;
            CompanyProfilePanel.Visibility = Visibility.Collapsed;
            PasswordChangePanel.Visibility = Visibility.Collapsed;
        }
        
        private void CompanyTabButton_Checked(object sender, RoutedEventArgs e)
        {
            if (UserProfilePanel == null || CompanyProfilePanel == null || PasswordChangePanel == null)
                return;
                
            UserProfilePanel.Visibility = Visibility.Collapsed;
            CompanyProfilePanel.Visibility = Visibility.Visible;
            PasswordChangePanel.Visibility = Visibility.Collapsed;
        }
        
        private void PasswordChangeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PasswordChangePanel == null || UserProfilePanel == null || CompanyProfilePanel == null)
                    return;
                    
                // Şifre değiştirme panelini göster/gizle
                if (PasswordChangePanel.Visibility == Visibility.Collapsed)
                {
                    PasswordChangePanel.Visibility = Visibility.Visible;
                    UserProfilePanel.Visibility = Visibility.Collapsed;
                    CompanyProfilePanel.Visibility = Visibility.Collapsed;
                    
                    // Formu temizle
                    if (CurrentPasswordBox != null) CurrentPasswordBox.Clear();
                    if (NewPasswordBox != null) NewPasswordBox.Clear();
                    if (ConfirmPasswordBox != null) ConfirmPasswordBox.Clear();
                    HidePasswordError();
                    
                    // Odağı şifre alanına ver
                    CurrentPasswordBox?.Focus();
                }
                else
                {
                    PasswordChangePanel.Visibility = Visibility.Collapsed;
                    UserProfilePanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Şifre değiştirme formu açılırken hata oluştu: {ex.Message}");
            }
        }
        
        private async void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentUser == null) return;
                
                // Şifre bilgilerini al
                string currentPassword = CurrentPasswordBox.Password;
                string newPassword = NewPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                
                // Boşluk kontrolü
                if (string.IsNullOrWhiteSpace(currentPassword) || 
                    string.IsNullOrWhiteSpace(newPassword) || 
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ShowPasswordError("Lütfen tüm şifre alanlarını doldurun.");
                    return;
                }
                
                // Şifre uzunluğu kontrolü
                if (newPassword.Length < 6)
                {
                    ShowPasswordError("Yeni şifre en az 6 karakter olmalıdır.");
                    return;
                }
                
                // Şifre eşleşme kontrolü
                if (newPassword != confirmPassword)
                {
                    ShowPasswordError("Yeni şifreler eşleşmiyor.");
                    return;
                }
                
                // Şifreyi değiştir
                bool result = await App.UserService.ChangePasswordAsync(_currentUser.Id, currentPassword, newPassword);
                
                if (result)
                {
                    MessageBox.Show("Şifreniz başarıyla değiştirildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Şifre değiştirme panelini gizle
                    PasswordChangePanel.Visibility = Visibility.Collapsed;
                    
                    // Hata mesajını temizle
                    HidePasswordError();
                }
                else
                {
                    ShowPasswordError("Mevcut şifreniz yanlış veya şifre değiştirme işlemi başarısız oldu.");
                }
            }
            catch (Exception ex)
            {
                ShowPasswordError($"Şifre değiştirme sırasında hata oluştu: {ex.Message}");
            }
        }
        
        private void CancelPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Şifre değiştirme panelini gizle
            PasswordChangePanel.Visibility = Visibility.Collapsed;
            
            // Hata mesajını temizle
            HidePasswordError();
        }
        
        // Yardımcı metotlar
        private void ShowError(string message)
        {
            if (ErrorMessage == null) return;
            
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
        
        private void HideError()
        {
            if (ErrorMessage == null) return;
            
            ErrorMessage.Text = string.Empty;
            ErrorMessage.Visibility = Visibility.Collapsed;
        }
        
        private void ShowCompanyError(string message)
        {
            if (CompanyErrorMessage == null) return;
            
            CompanyErrorMessage.Text = message;
            CompanyErrorMessage.Visibility = Visibility.Visible;
        }
        
        private void HideCompanyError()
        {
            if (CompanyErrorMessage == null) return;
            
            CompanyErrorMessage.Text = string.Empty;
            CompanyErrorMessage.Visibility = Visibility.Collapsed;
        }
        
        private void ShowPasswordError(string message)
        {
            if (PasswordErrorMessage == null) return;
            
            PasswordErrorMessage.Text = message;
            PasswordErrorMessage.Visibility = Visibility.Visible;
        }
        
        private void HidePasswordError()
        {
            if (PasswordErrorMessage == null) return;
            
            PasswordErrorMessage.Text = string.Empty;
            PasswordErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
    
    // Firma bilgilerini saklamak için yardımcı sınıf
    public class CompanyInfo
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }
} 