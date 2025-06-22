using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Dialogs;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// MaintenancePage.xaml için etkileşim mantığı
    /// </summary>
    public partial class MaintenancePage : Page
    {
        private List<Bakim> _bakimlar = new List<Bakim>();
        private Bakim? _selectedBakim;

        public MaintenancePage()
        {
            InitializeComponent();
            
            // Bu sayfa ilk yüklendiğinde yapılacaklar
            LoadMaintenanceRecords();
        }

        private async void LoadMaintenanceRecords()
        {
            try
            {
                _bakimlar = await App.BakimService.GetAllAsync();
                dgMaintenance.ItemsSource = _bakimlar;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bakım kayıtları yüklenirken hata oluştu: {ex.Message}", 
                              "Hata", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }
        
        // Yeni bakım kaydı ekleme işlemi için kullanılacak
        private void AddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MaintenanceRecordDialog(record: null, devices: new List<Device>(), maintenanceTypes: new List<string>());
            if (dialog.ShowDialog() == true)
            {
                LoadMaintenanceRecords();
            }
        }
        
        // Bakım detayları görüntüleme işlemi için kullanılacak
        private void ViewMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBakim != null)
            {
                var dialog = new MaintenanceRecordDialog(record: ConvertBakimToRecord(_selectedBakim), devices: new List<Device>(), maintenanceTypes: new List<string>());
                dialog.ShowDialog();
            }
        }
        
        // Bakım düzenleme işlemi için kullanılacak
        private void EditMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBakim != null)
            {
                var dialog = new MaintenanceRecordDialog(record: ConvertBakimToRecord(_selectedBakim), devices: new List<Device>(), maintenanceTypes: new List<string>());
                if (dialog.ShowDialog() == true)
                {
                    LoadMaintenanceRecords();
                }
            }
        }
        
        // Bakım'dan MaintenanceRecord'a dönüşüm için yardımcı metod
        private MaintenanceRecord ConvertBakimToRecord(Bakim bakim)
        {
            return new MaintenanceRecord
            {
                Id = bakim.Id,
                DeviceId = bakim.CihazId,
                MaintenanceDate = bakim.PlanlananTarih,
                NextMaintenanceDate = bakim.SonrakiTarih,
                MaintenanceType = bakim.BakimTuru,
                Status = bakim.Durum ?? (bakim.Tamamlandi ? "Tamamlandı" : "Planlandı"),
                Description = bakim.Aciklama,
                TechnicianName = bakim.Teknisyen,
                Cost = bakim.Maliyet,
                Notes = bakim.Notlar,
                CreatedAt = bakim.OlusturulmaTarihi,
                UpdatedAt = bakim.GuncellenmeTarihi,
                CreatedBy = bakim.OlusturanKullanici,
                UpdatedBy = bakim.GuncelleyenKullanici,
                IsCompleted = bakim.Tamamlandi,
                CompletedDate = bakim.GerceklesenTarih
            };
        }
        
        // Bakım tamamlama işlemi için kullanılacak
        private async void CompleteMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBakim != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bu bakım kaydını tamamlamak istediğinizden emin misiniz?", 
                    "Tamamlama Onayı", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await App.BakimService.BakimTamamlaAsync(_selectedBakim.Id, "Bakım tamamlandı");
                        if (success)
                        {
                            MessageBox.Show("Bakım başarıyla tamamlandı.", 
                                          "Başarılı", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Information);
                            LoadMaintenanceRecords();
                        }
                        else
                        {
                            MessageBox.Show("Bakım tamamlanırken bir hata oluştu.", 
                                          "Hata", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Bakım tamamlanırken hata oluştu: {ex.Message}", 
                                      "Hata", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dgMaintenance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBakim = dgMaintenance.SelectedItem as Bakim;
            btnViewMaintenance.IsEnabled = _selectedBakim != null;
            btnEditMaintenance.IsEnabled = _selectedBakim != null;
            btnCompleteMaintenance.IsEnabled = _selectedBakim != null && !_selectedBakim.Tamamlandi;
        }
    }
} 