using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Zimmet_Bakim_Takip.Models;
using System.Linq;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// PersonnelPage.xaml için etkileşim mantığı
    /// </summary>
    public partial class PersonnelPage : Page
    {
        private ObservableCollection<Personnel> _personnelList = new ObservableCollection<Personnel>();

        public PersonnelPage()
        {
            InitializeComponent();
            
            // Bu sayfa ilk yüklendiğinde yapılacaklar
            Loaded += PersonnelPage_Loaded;
        }
        
        private async void PersonnelPage_Loaded(object sender, RoutedEventArgs e)
        {
            // NavigationService event'ini sayfanın Loaded olayında ekle
            if (this.NavigationService != null)
            {
                this.NavigationService.Navigated += NavigationService_Navigated;
            }
            
            await LoadPersonnel();
        }
        
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            // Sayfaya tekrar dönüldüğünde personel listesini güncelle
            if (e.Content == this)
            {
                LoadPersonnel();
            }
        }

        private async Task LoadPersonnel()
        {
            try
            {
                // Veritabanından personel listesini çek
                var personnel = await App.PersonelService.GetAllAsync();
                
                // ObservableCollection'ı güncelle
                _personnelList.Clear();
                foreach (var person in personnel)
                {
                    _personnelList.Add(person);
                }
                
                // ListView'a bağla
                personnelListView.ItemsSource = _personnelList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel listesi yüklenirken hata oluştu: {ex.Message}", 
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Yeni personel ekleme işlemi için kullanılacak
        private void AddPersonnel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Frame içinde doğrudan yönlendirme
                var frame = Window.GetWindow(this).FindName("MainFrame") as Frame;
                if (frame != null)
                {
                    frame.Navigate(new Uri("/Pages/PersonnelAddPage.xaml", UriKind.Relative));
                }
                else
                {
                    // Alternatif yöntem: NavigationService ile
                    NavigationService?.Navigate(new Uri("/Pages/PersonnelAddPage.xaml", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel ekleme sayfasına geçiş yapılırken hata oluştu: {ex.Message}", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Personel detaylarını görüntüleme işlemi için kullanılacak
        private async void ViewPersonnel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Görüntülenecek personel ID'sini al
                if (sender is Button button && button.Tag != null)
                {
                    int personnelId = Convert.ToInt32(button.Tag);
                    
                    // Personel bilgilerini çek
                    var personnel = await App.PersonelService.GetByIdAsync(personnelId);
                    
                    if (personnel == null)
                    {
                        MessageBox.Show("Personel bilgisi bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    
                    // Personel bilgilerini göster
                    string details = $"Ad: {personnel.FirstName}\n" +
                                     $"Soyad: {personnel.LastName}\n" +
                                     $"Departman: {personnel.Department}\n" +
                                     $"Pozisyon: {personnel.Position}\n" +
                                     $"E-posta: {personnel.Email}\n" +
                                     $"Telefon: {personnel.Phone}\n" +
                                     $"Adres: {personnel.Address}\n" +
                                     $"Durum: {(personnel.IsActive ? "Aktif" : "Pasif")}\n" +
                                     $"Oluşturulma Tarihi: {personnel.CreatedAt:dd.MM.yyyy HH:mm}\n" +
                                     $"Son Güncelleme: {personnel.UpdatedAt:dd.MM.yyyy HH:mm}";
                    
                    MessageBox.Show(details, $"Personel Detayları - {personnel.FullName}", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Personel bilgisi alınamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel bilgileri görüntülenirken hata oluştu: {ex.Message}", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Personel düzenleme işlemi için kullanılacak
        private void EditPersonnel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Düzenlenecek personel ID'sini al
                if (sender is Button button && button.Tag != null)
                {
                    int personnelId = Convert.ToInt32(button.Tag);
                    
                    // Düzenleme sayfasına yönlendir
                    if (NavigationService != null)
                    {
                        NavigationService.Navigate(new PersonnelEditPage(personnelId));
                    }
                    else
                    {
                        // Alternatif yöntem: Frame ile
                        var frame = Window.GetWindow(this).FindName("MainFrame") as Frame;
                        if (frame != null)
                        {
                            frame.Navigate(new PersonnelEditPage(personnelId));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Personel bilgisi alınamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel düzenleme sayfasına geçiş yapılırken hata oluştu: {ex.Message}", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Personel silme işlemi için kullanılacak
        private async void DeletePersonnel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // İlgili personel ID'sini al
                if (sender is Button button && button.Tag != null)
                {
                    int personnelId = Convert.ToInt32(button.Tag);
                    
                    // Kullanıcıya onay sor
                    MessageBoxResult result = MessageBox.Show("Bu personeli silmek istediğinizden emin misiniz?", 
                                                           "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // Personel silme işlemini gerçekleştir
                        bool success = await App.PersonelService.DeleteAsync(personnelId);
                        
                        if (success)
                        {
                            MessageBox.Show("Personel başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Listeyi güncelle
                            await LoadPersonnel();
                        }
                        else
                        {
                            MessageBox.Show("Personel silinirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Personel bilgisi alınamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel silinirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Arama kutusu için event handler
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Arama işlemini gerçekleştir
            string searchTerm = txtSearch.Text.Trim().ToLower();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                // Arama terimi boşsa tüm listeyi göster
                personnelListView.ItemsSource = _personnelList;
                return;
            }
            
            // Arama terimine göre filtrele
            var filteredList = new ObservableCollection<Personnel>(
                _personnelList.Where(p => 
                    p.FirstName.ToLower().Contains(searchTerm) || 
                    p.LastName.ToLower().Contains(searchTerm) || 
                    p.Department?.ToLower().Contains(searchTerm) == true ||
                    p.Email?.ToLower().Contains(searchTerm) == true
                )
            );
            
            personnelListView.ItemsSource = filteredList;
        }
    }
} 