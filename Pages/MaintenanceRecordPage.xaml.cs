using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Dialogs;
using System.Windows.Data;

namespace Zimmet_Bakim_Takip.Pages
{
    public partial class MaintenanceRecordPage : Page
    {
        private MaintenanceRecord? _selectedRecord;
        private List<Device>? _devices;
        private List<string> _maintenanceTypes = new List<string>
        {
            "Periyodik Bakım",
            "Arıza Giderme",
            "Yazılım Güncelleme",
            "Parça Değişimi",
            "Kalibrasyon",
            "Diğer"
        };

        public MaintenanceRecordPage()
        {
            InitializeComponent();
            
            // _devices listesini başlat
            _devices = new List<Device>();
            
            // Bakım türleri listesini başlat
            _maintenanceTypes = new List<string>
            {
                "Periyodik Bakım",
                "Arıza Giderme",
                "Yazılım Güncelleme",
                "Parça Değişimi",
                "Kalibrasyon",
                "Diğer"
            };
            
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var records = await App.MaintenanceService.GetAllMaintenanceRecordsAsync();
                dgMaintenanceRecords.ItemsSource = records;

                // Cihazları yükle
                _devices = await App.CihazService.GetAllDevicesAsync();
                
                // Cihazlar yüklendikten sonra filtreleri başlat
                InitializeFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bakım kayıtları yüklenirken hata oluştu: {ex.Message}", 
                                "Veri Yükleme Hatası", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }

        private void InitializeFilters()
        {
            cmbFilterDevice.ItemsSource = _devices;
            cmbFilterMaintenanceType.ItemsSource = _maintenanceTypes;
            
            // Varsayılan tarihler
            dpFilterStartDate.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterEndDate.SelectedDate = DateTime.Today;
        }

        private void dgMaintenanceRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRecord = dgMaintenanceRecords.SelectedItem as MaintenanceRecord;
            btnEditMaintenanceRecord.IsEnabled = _selectedRecord != null;
            btnDeleteMaintenanceRecord.IsEnabled = _selectedRecord != null;
        }

        private void btnNewMaintenanceRecord_Click(object sender, RoutedEventArgs e)
        {
            // _devices null olabilir korumaya alıyorum
            var devicesList = _devices ?? new List<Device>();
            var dialog = new MaintenanceRecordDialog(record: null, devices: devicesList, maintenanceTypes: _maintenanceTypes);
            if (dialog.ShowDialog() == true)
            {
                RefreshData();
            }
        }

        private void btnEditMaintenanceRecord_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRecord != null)
            {
                // _devices null olabilir korumaya alıyorum
                var devicesList = _devices ?? new List<Device>();
                var dialog = new MaintenanceRecordDialog(record: _selectedRecord, devices: devicesList, maintenanceTypes: _maintenanceTypes);
                if (dialog.ShowDialog() == true)
                {
                    RefreshData();
                }
            }
        }

        private async void btnDeleteMaintenanceRecord_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRecord != null)
            {
                var result = MessageBox.Show(
                    $"Seçili bakım kaydını silmek istediğinize emin misiniz?\n\nCihaz: {_selectedRecord.Device?.Name}\nTarih: {_selectedRecord.MaintenanceDate:dd.MM.yyyy}", 
                    "Silme Onayı", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await App.MaintenanceService.DeleteMaintenanceRecordAsync(_selectedRecord.Id);
                        RefreshData();
                        MessageBox.Show("Bakım kaydı başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Bakım kaydı silinirken hata oluştu: {ex.Message}", "Silme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            LoadData();
            _selectedRecord = null;
            btnEditMaintenanceRecord.IsEnabled = false;
            btnDeleteMaintenanceRecord.IsEnabled = false;
        }

        private async void btnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tüm kayıtları al
                var allRecords = await App.MaintenanceService.GetAllMaintenanceRecordsAsync();
                
                // Filtreleme kriterlerini uygula
                var filteredRecords = allRecords.AsEnumerable();

                // Cihaz filtresi
                if (cmbFilterDevice.SelectedItem is Device selectedDevice)
                {
                    filteredRecords = filteredRecords.Where(r => r.DeviceId == selectedDevice.Id);
                }

                // Bakım tipi filtresi
                if (cmbFilterMaintenanceType.SelectedItem is string selectedType)
                {
                    filteredRecords = filteredRecords.Where(r => r.MaintenanceType == selectedType);
                }

                // Tarih aralığı filtresi
                if (dpFilterStartDate.SelectedDate.HasValue)
                {
                    var startDate = dpFilterStartDate.SelectedDate.Value.Date;
                    filteredRecords = filteredRecords.Where(r => r.MaintenanceDate >= startDate);
                }

                if (dpFilterEndDate.SelectedDate.HasValue)
                {
                    var endDate = dpFilterEndDate.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);
                    filteredRecords = filteredRecords.Where(r => r.MaintenanceDate <= endDate);
                }

                // Sonuçları DataGrid'e uygula
                dgMaintenanceRecords.ItemsSource = filteredRecords.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtre uygulanırken hata oluştu: {ex.Message}", "Filtre Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterDevice.SelectedItem = null;
            cmbFilterMaintenanceType.SelectedItem = null;
            dpFilterStartDate.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFilterEndDate.SelectedDate = DateTime.Today;
            
            RefreshData();
        }
    }
} 