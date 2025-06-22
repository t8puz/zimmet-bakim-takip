using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Zimmet_Bakim_Takip.Utilities;
using System.IO;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// Paylaşımlı veritabanı ayarları sayfası
    /// </summary>
    public partial class SharedDatabaseSettingsPage : Page
    {
        private SharedDatabaseSettings _settings;
        
        public SharedDatabaseSettingsPage()
        {
            InitializeComponent();
            
            // Mevcut ayarları yükle
            _settings = SharedDatabaseSettings.Instance;
            
            // UI'ı ayarlarla güncelle
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            // Checkbox durumu
            UseSharedDatabaseCheckbox.IsChecked = _settings.UseSharedDatabase;
            
            // Paylaşımlı klasör yolu
            SharedFolderPathTextBox.Text = _settings.SharedFolderPath;
            
            // Son güncelleme zamanı
            if (_settings.LastUpdated > DateTime.MinValue)
            {
                LastUpdatedText.Text = _settings.LastUpdated.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                LastUpdatedText.Text = "Hiç";
            }
            
            // Bilgi metni
            UpdateInfoText();
        }
        
        private void UpdateInfoText()
        {
            if (UseSharedDatabaseCheckbox.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(SharedFolderPathTextBox.Text))
                {
                    InfoTextBlock.Text = "Lütfen bir paylaşımlı klasör seçin.";
                }
                else if (!Directory.Exists(SharedFolderPathTextBox.Text))
                {
                    InfoTextBlock.Text = "Seçilen klasör bulunamadı. Lütfen geçerli bir klasör seçin.";
                }
                else
                {
                    InfoTextBlock.Text = "Paylaşımlı veritabanı kullanılacak. Bu ayarlar kaydedildikten sonra uygulama yeniden başlatılmalıdır.";
                }
            }
            else
            {
                InfoTextBlock.Text = "Yerel veritabanı kullanılacak. Her bilgisayar kendi veritabanını kullanır.";
            }
        }
        
        private void UseSharedDatabaseCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateInfoText();
        }
        
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Windows API Code Pack ile klasör seçim diyaloğu
            var dialog = new CommonOpenFileDialog
            {
                Title = "Veritabanı için paylaşımlı klasör seçin",
                IsFolderPicker = true,
                InitialDirectory = !string.IsNullOrEmpty(SharedFolderPathTextBox.Text) && Directory.Exists(SharedFolderPathTextBox.Text) 
                                   ? SharedFolderPathTextBox.Text 
                                   : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SharedFolderPathTextBox.Text = dialog.FileName;
                UpdateInfoText();
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ayarları güncelle
                _settings.UseSharedDatabase = UseSharedDatabaseCheckbox.IsChecked == true;
                _settings.SharedFolderPath = SharedFolderPathTextBox.Text;
                
                // Paylaşımlı klasör kullanılacaksa, klasörün erişilebilir olduğunu kontrol et
                if (_settings.UseSharedDatabase && !string.IsNullOrEmpty(_settings.SharedFolderPath))
                {
                    if (!Directory.Exists(_settings.SharedFolderPath))
                    {
                        MessageBox.Show("Seçilen klasör bulunamadı. Lütfen geçerli bir klasör seçin.", 
                            "Klasör Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Yazma yetkisi kontrol et
                    try
                    {
                        string testFile = Path.Combine(_settings.SharedFolderPath, "test_write.tmp");
                        File.WriteAllText(testFile, "Test");
                        File.Delete(testFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Seçilen klasöre yazma yetkisi yok. Hata: {ex.Message}", 
                            "Yetki Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                
                // Ayarları kaydet
                bool saved = _settings.Save();
                
                if (saved)
                {
                    // UI'ı güncelle
                    UpdateUI();
                    
                    // Bilgi mesajı göster
                    MessageBox.Show("Ayarlar başarıyla kaydedildi. Değişikliklerin etkili olması için uygulamayı yeniden başlatın.", 
                        "Ayarlar Kaydedildi", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Uygulamayı yeniden başlatmak isteyip istemediğini sor
                    var restartResult = MessageBox.Show("Uygulamayı şimdi yeniden başlatmak ister misiniz?", 
                        "Uygulama Yeniden Başlatma", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (restartResult == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    MessageBox.Show("Ayarlar kaydedilirken bir hata oluştu.", 
                        "Kaydetme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar kaydedilirken bir hata oluştu: {ex.Message}", 
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Değişiklikleri iptal et ve bir önceki sayfaya dön
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }
    }
} 