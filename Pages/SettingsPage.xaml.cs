using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using Zimmet_Bakim_Takip.Utilities;
using System.Windows.Threading;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// SettingsPage.xaml için etkileşim mantığı
    /// </summary>
    public partial class SettingsPage : Page
    {
        // Reset işlemi için flag dosyasının adı
        private const string RESET_FLAG_FILENAME = "reset_db.flag";
        
        // Firma bilgileri sınıfı
        private SettingsCompanyInfo _companyInfo = new SettingsCompanyInfo();
        
        public SettingsPage()
        {
            InitializeComponent();
            
            // Ayarlar yüklenecek
            LoadSettings();
            
            // Firma bilgilerini yükle
            LoadCompanyInfo();
            
            // Mevcut tema ayarını göster
            UpdateThemeSelection();
            
            // Tema değişikliği olayını dinle
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            
            // Sayfa kapandığında olayları temizle
            this.Unloaded += SettingsPage_Unloaded;
        }
        
        private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Sayfa kapandığında olayları temizle
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
        }
        
        private void ThemeManager_ThemeChanged(object sender, AppTheme theme)
        {
            // Tema değiştiğinde sayfadaki kontrolleri güncelle
            UpdateControlsAppearance();
        }
        
        private void UpdateControlsAppearance()
        {
            // Sayfadaki tüm kontrolleri tema değişikliğine uygun şekilde güncelle
            // RadioButtons, TextBoxes ve diğer kontroller için görsel güncellemeleri burada yapabiliriz
            
            // ToggleButton stillerini zorla güncelle
            foreach (var toggleButton in FindVisualChildren<ToggleButton>(this))
            {
                var originalState = toggleButton.IsChecked;
                
                // Stil değişikliğini tetiklemek için değeri geçici olarak değiştir
                toggleButton.IsChecked = !originalState;
                toggleButton.IsChecked = originalState;
            }
            
            // RadioButton stillerini zorla güncelle
            foreach (var radioButton in FindVisualChildren<RadioButton>(this))
            {
                if (radioButton.Style != null && radioButton.Style.ToString().Contains("DefaultRadioButtonStyle"))
                {
                    radioButton.Style = Application.Current.Resources["DefaultRadioButtonStyle"] as Style;
                }
                
                // Metin rengini güncelle
                radioButton.SetResourceReference(RadioButton.ForegroundProperty, "PrimaryTextBrush");
            }
        }
        
        // UI ağacındaki tüm belirli tipte kontrolleri bul
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        
        private void LoadSettings()
        {
            // Burada ayarlar yüklenecek
            // Örnek: Sistem ayarlarını yükle
            
            // Şimdilik örnek veriler kullanılıyor
            // Gerçek uygulamada yapılandırma dosyası veya veritabanından yüklenecek
        }
        
        private void UpdateThemeSelection()
        {
            // Mevcut tema ayarını göster
            if (ThemeManager.CurrentTheme == AppTheme.Light)
            {
                LightThemeRadio.IsChecked = true;
            }
            else
            {
                DarkThemeRadio.IsChecked = true;
            }
        }
        
        // Açık tema seçildiğinde
        private void LightThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && LightThemeRadio.IsChecked == true)
            {
                // Tema değişimi artık basit bir metot çağrısı
                if (ThemeManager.CurrentTheme != AppTheme.Light)
                {
                    ThemeManager.ChangeTheme(AppTheme.Light);
                }
            }
        }
        
        // Koyu tema seçildiğinde
        private void DarkThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && DarkThemeRadio.IsChecked == true)
            {
                // Tema değişimi artık basit bir metot çağrısı
                if (ThemeManager.CurrentTheme != AppTheme.Dark)
                {
                    ThemeManager.ChangeTheme(AppTheme.Dark);
                }
            }
        }
        
        // Veritabanı yedekleme işlemi
        private async void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Dosya konumu seçme diyaloğunu aç
                var saveDialog = new CommonSaveFileDialog
                {
                    Title = "Veritabanı yedeği nereye kaydedilsin?",
                    DefaultFileName = $"ZimmetBakim_Yedek_{DateTime.Now:yyyyMMdd_HHmmss}.zbkp",
                    DefaultExtension = "zbkp",
                    AlwaysAppendDefaultExtension = true
                };
                
                saveDialog.Filters.Add(new CommonFileDialogFilter("Zimmet Bakım Yedekleri", "*.zbkp"));

                if (saveDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    // Kullanıcı arayüzünü etkileşimsiz yap
                    this.IsEnabled = false;
                    var cursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = Cursors.Wait;
                    
                    try
                    {
                        // Veri yedekleme servisini kullan
                        var result = await App.DataBackupService.BackupDatabaseAsync(saveDialog.FileName);
                        
                        if (result.Success)
                        {
                            MessageBox.Show($"Veritabanı başarıyla yedeklendi:\n{result.FilePath}", 
                                "Yedekleme Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Veritabanı yedeklenemedi:\n{result.Message}", 
                                "Yedekleme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    finally
                    {
                        // Kullanıcı arayüzünü tekrar aktif et
                        this.IsEnabled = true;
                        Mouse.OverrideCursor = cursor;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yedekleme işlemi sırasında hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Veritabanı geri yükleme işlemi
        private async void RestoreDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kullanıcıyı uyar
                var warningResult = MessageBox.Show(
                    "Veritabanını geri yüklemek mevcut verilerin üzerine yazacaktır. Devam etmek istiyor musunuz?",
                    "Uyarı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (warningResult != MessageBoxResult.Yes)
                {
                    return;
                }
                
                // Dosya seçme diyaloğunu aç
                var openDialog = new CommonOpenFileDialog
                {
                    Title = "Geri yüklenecek veritabanı yedeğini seçin",
                    DefaultExtension = "zbkp",
                    EnsureFileExists = true,
                    Multiselect = false
                };
                
                openDialog.Filters.Add(new CommonFileDialogFilter("Zimmet Bakım Yedekleri", "*.zbkp"));

                if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    // Kullanıcı arayüzünü etkileşimsiz yap
                    this.IsEnabled = false;
                    var cursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = Cursors.Wait;
                    
                    try
                    {
                        // Veri geri yükleme servisini kullan
                        var result = await App.DataBackupService.RestoreDatabaseAsync(openDialog.FileName);
                        
                        if (result.Success)
                        {
                            var restartResult = MessageBox.Show(
                                $"{result.Message}\n\nDeğişikliklerin etkili olması için uygulama yeniden başlatılmalıdır. Şimdi yeniden başlatmak istiyor musunuz?", 
                                "Geri Yükleme Başarılı", MessageBoxButton.YesNo, MessageBoxImage.Information);
                                
                            if (restartResult == MessageBoxResult.Yes)
                            {
                                // Uygulamayı yeniden başlat
                                System.Diagnostics.Process.Start(
                                    System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                                Application.Current.Shutdown();
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Veritabanı geri yüklenemedi:\n{result.Message}", 
                                "Geri Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    finally
                    {
                        // Kullanıcı arayüzünü tekrar aktif et
                        this.IsEnabled = true;
                        Mouse.OverrideCursor = cursor;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Geri yükleme işlemi sırasında hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Örnek verileri temizleme işlemi
        private async void ClearSampleData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kullanıcıdan onay al
                MessageBoxResult result = MessageBox.Show(
                    "Örnek verileri temizlemek istediğinizden emin misiniz?\n\n" +
                    "Bu işlem şunları silecektir:\n" +
                    "• Tüm cihaz kayıtları\n" +
                    "• Tüm personel kayıtları\n" +
                    "• Tüm zimmet kayıtları\n" +
                    "• Tüm bakım kayıtları\n" +
                    "• Tüm ek dosyalar\n" +
                    "• Admin dışındaki kullanıcılar\n\n" +
                    "Bu işlem geri alınamaz ancak otomatik yedek alınacaktır.", 
                    "Örnek Verileri Temizle", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                    
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Kullanıcı arayüzünü etkileşimsiz yap
                this.IsEnabled = false;
                var cursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    using (var scope = ((App)Application.Current).Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var dataCleaningService = new DataCleaningService(context);

                        var clearResult = await dataCleaningService.ClearAllSampleDataAsync();

                        if (clearResult.Success)
                        {
                            MessageBox.Show(
                                clearResult.Message,
                                "Temizleme Başarılı",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                clearResult.Message,
                                "Temizleme Hatası",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                finally
                {
                    // Kullanıcı arayüzünü tekrar aktif et
                    this.IsEnabled = true;
                    Mouse.OverrideCursor = cursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Örnek verileri temizleme işlemi sırasında hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Veri durumunu görüntüleme işlemi
        private async void ViewDataSummary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kullanıcı arayüzünü etkileşimsiz yap
                this.IsEnabled = false;
                var cursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    using (var scope = ((App)Application.Current).Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var dataCleaningService = new DataCleaningService(context);

                        var summary = await dataCleaningService.GetDataSummaryAsync();

                        MessageBox.Show(
                            summary,
                            "Veri Durumu",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                finally
                {
                    // Kullanıcı arayüzünü tekrar aktif et
                    this.IsEnabled = true;
                    Mouse.OverrideCursor = cursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri durumu alınırken hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Verileri sıfırlama işlemi
        private async void ResetData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kullanıcıdan onay al
                MessageBoxResult result = MessageBox.Show(
                    "Tüm verileri sıfırlamak istediğinizden emin misiniz? Bu işlem geri alınamaz ve tüm kayıtlar silinecektir.", 
                    "Uyarı", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                    
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
                
                // İkinci kez onay al
                result = MessageBox.Show(
                    "DİKKAT: Son onay! Bu işlem tüm verileri silecek ve geri alınamaz. Devam etmek istediğinizden emin misiniz?", 
                    "Son Uyarı", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Stop);
                    
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
                
                // Kullanıcı arayüzünü etkileşimsiz yap
                this.IsEnabled = false;
                var cursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
                
                try
                {
                    // Önce mevcut veritabanını yedekle
                    var backupResult = await App.DataBackupService.BackupDatabaseAsync();
                    
                    if (!backupResult.Success)
                    {
                        MessageBox.Show(
                            "Mevcut veritabanı yedeklenemedi. Güvenlik nedeniyle sıfırlama işlemi iptal edildi.", 
                            "Sıfırlama İptal Edildi", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return;
                    }

                    // Sıfırlama bayrağı dosyasının tam yolu
                    string flagFilePath = Path.Combine(
                        AppDbContextFactory.GetDataFolderPath(), 
                        RESET_FLAG_FILENAME);
                    
                    try
                    {
                        // Yedek dosya yolunu bayrağa yaz
                        File.WriteAllText(flagFilePath, backupResult.FilePath);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(
                            $"Bayrak dosyası oluşturulamadı: {ex.Message}. Sıfırlama işlemi iptal edildi.", 
                            "Hata", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return;
                    }
                    
                    // Kullanıcıya bilgi ver
                    MessageBox.Show(
                        "Veritabanı sıfırlama işlemi uygulamanın yeniden başlatılmasıyla gerçekleştirilecek.", 
                        "Bilgi", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    // Uygulamayı yeniden başlat
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Application.Current.Shutdown();
                }
                finally
                {
                    // Kullanıcı arayüzünü tekrar aktif et
                    this.IsEnabled = true;
                    Mouse.OverrideCursor = cursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Verileri sıfırlama işlemi sırasında hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Uygulama başlangıcında bir kez çağrılarak veritabanı sıfırlama flagı varsa
        /// sıfırlama işlemini gerçekleştirir
        /// </summary>
        public static async Task CheckAndResetDatabaseIfNeeded()
        {
            try
            {
                // Flag dosyasının yolunu al
                string flagFilePath = Path.Combine(
                    AppDbContextFactory.GetDataFolderPath(), 
                    RESET_FLAG_FILENAME);
                
                // Flag yoksa işlem yapma
                if (!File.Exists(flagFilePath))
                {
                    return;
                }
                
                // Yedek dosya yolunu oku
                string backupPath = File.ReadAllText(flagFilePath);
                
                // Veritabanı yolunu al
                string dbPath = AppDbContextFactory.GetDbPath();
                
                // Bu noktada uygulama yeni başlatıldı, veritabanı dosyası kilitli değil
                try
                {
                    // Veritabanına erişim kontrolü - kilitlenme sorunlarına karşı
                    if (File.Exists(dbPath))
                    {
                        try
                        {
                            // Dosyayı silmeyi dene - eğer kilitliyse exception fırlatır
                            bool isLocked = false;
                            try
                            {
                                // Dosyayı geçici olarak açmaya çalış
                                using (FileStream stream = new FileStream(dbPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                {
                                    // Hiçbir şey yapma, sadece dosyaya erişilebildiğini kontrol et
                                }
                            }
                            catch (IOException)
                            {
                                isLocked = true;
                            }
                            
                            if (isLocked)
                            {
                                MessageBox.Show(
                                    "Veritabanı dosyası başka bir süreç tarafından kullanılıyor. Lütfen diğer uygulamaları kapatıp tekrar deneyin.", 
                                    "Veritabanı Kilitli", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                                
                                // Flag dosyasını silme - kullanıcının tekrar denemesi için
                                return;
                            }
                            
                            // Veritabanını sil
                            File.Delete(dbPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Veritabanı silinemedi: {ex.Message}. Lütfen uygulamayı yeniden başlatın.", 
                                "Sıfırlama Hatası", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                            
                            // Hata durumunda flag dosyasını koruyalım
                            return;
                        }
                    }
                    
                    // Yeni veritabanı oluştur
                    using (var scope = ((App)Application.Current).Services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        
                        // Bağlantı açıksa kapat (güvenli olmak için)
                        await context.Database.CloseConnectionAsync();
                        
                        // Veritabanını yeniden oluştur
                        await context.Database.EnsureCreatedAsync();
                    }
                    
                    // İşlem başarılı, kullanıcıya bilgi ver
                    MessageBox.Show(
                        $"Veritabanı başarıyla sıfırlandı.\nYedeğe alınan eski veritabanı: {backupPath}", 
                        "Sıfırlama Başarılı", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    // Flag dosyasını sil
                    File.Delete(flagFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Veritabanı sıfırlanırken hata oluştu: {ex.Message}", 
                        "Sıfırlama Hatası", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    
                    // Hata durumunda flag dosyasını sil
                    if (File.Exists(flagFilePath))
                    {
                        File.Delete(flagFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sıfırlama kontrolü hatası: {ex.Message}");
                
                // Hata olursa flag dosyası varsa sil
                try
                {
                    string flagFilePath = Path.Combine(
                        AppDbContextFactory.GetDataFolderPath(), 
                        RESET_FLAG_FILENAME);
                    
                    if (File.Exists(flagFilePath))
                    {
                        File.Delete(flagFilePath);
                    }
                }
                catch { /* Ignore */ }
            }
        }
        
        // Kullanıcıları yönet butonu tıklandığında
        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame üzerinden kullanıcı yönetimi sayfasına git
            if (this.Parent is Frame mainFrame)
            {
                mainFrame.Navigate(new UserManagementPage());
            }
        }
        
        // Paylaşımlı veritabanı ayarları butonuna tıklandığında
        private void SharedDatabaseSettings_Click(object sender, RoutedEventArgs e)
        {
            // MainFrame üzerinden paylaşımlı veritabanı ayarları sayfasına git
            if (this.Parent is Frame mainFrame)
            {
                mainFrame.Navigate(new SharedDatabaseSettingsPage());
            }
        }
        
        // Ayarları kaydetme işlemi
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Ayarları kaydet (dil, tema, bildirimler, vs.)
            
            // Burada diğer ayarları da kaydetme işlemi olacak
            
            MessageBox.Show("Ayarlar başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        // Firma bilgilerini yükleme
        private void LoadCompanyInfo()
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
                        _companyInfo = JsonSerializer.Deserialize<SettingsCompanyInfo>(json) ?? new SettingsCompanyInfo();
                    }
                }
                
                // Formdaki alanları doldur
                CompanyNameTextBox.Text = _companyInfo.CompanyName;
                CompanyAddressTextBox.Text = _companyInfo.Address;
                CompanyContactTextBox.Text = _companyInfo.Contact;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Firma bilgileri yüklenirken hata: {ex.Message}");
                MessageBox.Show($"Firma bilgileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Firma adı baş harflerini alma
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
        
        private void SaveCompanyInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Formdaki değerleri al
                string companyName = CompanyNameTextBox.Text?.Trim() ?? "";
                string address = CompanyAddressTextBox.Text?.Trim() ?? "";
                string contact = CompanyContactTextBox.Text?.Trim() ?? "";
                
                // Firma bilgilerini güncelle
                _companyInfo.CompanyName = companyName;
                _companyInfo.Address = address;
                _companyInfo.Contact = contact;
                
                // Firma bilgilerini dosyaya kaydet
                SaveCompanyInfoToFile();
                
                // Ana penceredeki firma adını güncelle
                UpdateMainWindowCompanyInfo();
                
                MessageBox.Show("Firma bilgileri başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Firma bilgileri kaydedilirken hata: {ex.Message}");
                MessageBox.Show($"Firma bilgileri kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Firma bilgilerini dosyaya kaydetme
        private void SaveCompanyInfoToFile()
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
        
        /// <summary>
        /// E-posta bağlantısına tıklandığında varsayılan e-posta istemcisini açar
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                // E-posta bağlantısına tıklandığında varsayılan e-posta istemcisini aç
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"E-posta uygulaması açılırken hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        
                        // Firma bilgilerini güncelle
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ana pencere firma bilgileri güncellenirken hata: {ex.Message}");
            }
        }
    }
    
    // Firma bilgilerini saklamak için yardımcı sınıf
    public class SettingsCompanyInfo
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }
} 