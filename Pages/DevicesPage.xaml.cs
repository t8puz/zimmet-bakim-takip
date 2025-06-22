using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zimmet_Bakim_Takip.Dialogs;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;
using Zimmet_Bakim_Takip.Utilities;

namespace Zimmet_Bakim_Takip.Pages
{
    public partial class DevicesPage : Page
    {
        private List<Device> allDevices = new List<Device>();
        private List<Device> filteredDevices = new List<Device>();
        private int currentPage = 1;
        private const int ItemsPerPage = 10;
        private string currentSearchText = string.Empty;
        private readonly ICihazService _cihazService;
        private bool _isLoading = false;
        private bool isLoading 
        { 
            get => _isLoading; 
            set 
            {
                _isLoading = value;
                UpdateLoadingVisibility();
            }
        }

        public DevicesPage()
        {
            InitializeComponent();

            // Servis referansını al
            _cihazService = App.CihazService;

            // Cihazları yükle
            Loaded += async (s, e) => await LoadDevicesAsync();
        }

        private async Task LoadDevicesAsync()
        {
            try
            {
                isLoading = true;
                
                // Veritabanından cihazları çek
                allDevices = await _cihazService.GetAllDevicesAsync();
                
                // Arama filtresi uygula
                ApplyFilter();
                
                // Sayfalamayı güncelle
                UpdatePagination();
                
                isLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cihazlar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                isLoading = false;
            }
        }

        private void ApplyFilter()
        {
            // Arama metnine göre filtrele
            if (string.IsNullOrWhiteSpace(currentSearchText))
            {
                filteredDevices = allDevices.ToList();
            }
            else
            {
                filteredDevices = allDevices
                    .Where(d => (d.Name != null && d.Name.Contains(currentSearchText, StringComparison.OrdinalIgnoreCase)) ||
                               (d.SerialNumber != null && d.SerialNumber.Contains(currentSearchText, StringComparison.OrdinalIgnoreCase)) ||
                               (d.Type != null && d.Type.Contains(currentSearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // Geçerli sayfayı güncelle
            currentPage = 1;
            
            // UI'ı güncelle
            UpdateDevicesGridDisplay();
            UpdatePagination();
        }

        private void UpdateDevicesGridDisplay()
        {
            // Mevcut sayfadaki öğeleri seç
            int skipCount = (currentPage - 1) * ItemsPerPage;
            var currentPageItems = filteredDevices
                .Skip(skipCount)
                .Take(ItemsPerPage)
                .ToList();

            // DataGrid'e bağla
            DevicesGrid.ItemsSource = currentPageItems;

            // "Veri yok" panelini güncelle
            NoDataPanel.Visibility = filteredDevices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            // Toplam sayısını güncelle
            TotalDevicesCount.Text = $"{filteredDevices.Count} Cihaz";
        }

        private void UpdatePagination()
        {
            // Toplam sayfa sayısını hesapla
            int totalPages = (int)Math.Ceiling(filteredDevices.Count / (double)ItemsPerPage);
            if (totalPages == 0) totalPages = 1;

            // Geçerli sayfanın geçerli aralıkta olduğundan emin ol
            if (currentPage > totalPages) currentPage = totalPages;

            // Sayfa bilgisini güncelle
            PageInfoText.Text = $"Sayfa {currentPage} / {totalPages}";

            // Önceki/sonraki sayfa butonlarını güncelle
            PreviousPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled = currentPage < totalPages;
        }

        private void UpdateLoadingVisibility()
        {
            if (_isLoading)
            {
                LoadingPanel.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        #region Event Handlers

        private void SearchBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            currentSearchText = SearchBox.Text?.Trim() ?? string.Empty;
            ApplyFilter();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            currentSearchText = string.Empty;
            ApplyFilter();
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateDevicesGridDisplay();
                UpdatePagination();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling(filteredDevices.Count / (double)ItemsPerPage);
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateDevicesGridDisplay();
                UpdatePagination();
            }
        }

        private void DevicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Seçili cihazı al
            var selectedDevice = DevicesGrid.SelectedItem as Device;
            
            // Gerekirse burada seçili cihaz bilgilerini göster veya işle
        }

        private async void AddDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading) return;
            
            // Yeni cihaz ekleme formunu göster
            var dialog = new DeviceEditDialog();
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    isLoading = true;
                    
                    // Yeni cihazı veritabanına ekle
                    var newDevice = dialog.Device;
                    var result = await _cihazService.AddDeviceAsync(newDevice);
                    
                    if (result)
                    {
                        // Listeyi güncelle
                        await LoadDevicesAsync();
                        MessageBox.Show("Cihaz başarıyla eklendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Cihaz eklenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                    isLoading = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cihaz eklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    isLoading = false;
                }
            }
        }

        private async void EditDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading) return;
            
            // Tıklanan cihazın ID'sini al
            if (sender is Button button && button.Tag is int deviceId)
            {
                isLoading = true;
                
                // İlgili cihazı servis aracılığıyla bul
                var device = await _cihazService.GetDeviceByIdAsync(deviceId);
                isLoading = false;
                
                if (device != null)
                {
                    // Düzenleme formunu göster
                    var dialog = new DeviceEditDialog(device);
                    dialog.Owner = Window.GetWindow(this);
                    
                    if (dialog.ShowDialog() == true)
                    {
                        try
                        {
                            isLoading = true;
                            
                            // Cihaz bilgilerini güncelle
                            var result = await _cihazService.UpdateDeviceAsync(device);
                            
                            if (result)
                            {
                                // Listeyi güncelle
                                await LoadDevicesAsync();
                                MessageBox.Show("Cihaz başarıyla güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Cihaz güncellenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            
                            isLoading = false;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Cihaz güncellenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            isLoading = false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Cihaz bulunamadı. Lütfen sayfayı yenileyip tekrar deneyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DeleteDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading) return;
            
            // Tıklanan cihazın ID'sini al
            if (sender is Button button && button.Tag is int deviceId)
            {
                isLoading = true;
                
                // İlgili cihazı servis aracılığıyla bul
                var device = await _cihazService.GetDeviceByIdAsync(deviceId);
                isLoading = false;
                
                if (device != null)
                {
                    // Silme onayını sor
                    var result = MessageBox.Show($"'{device.Name}' cihazını silmek istediğinizden emin misiniz?", 
                        "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            isLoading = true;
                            
                            // Cihazı sil
                            var success = await _cihazService.DeleteDeviceAsync(deviceId);
                            
                            if (success)
                            {
                                // Listeyi güncelle
                                await LoadDevicesAsync();
                                MessageBox.Show("Cihaz başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Cihaz silinirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            
                            isLoading = false;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Cihaz silinirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            isLoading = false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Cihaz bulunamadı. Lütfen sayfayı yenileyip tekrar deneyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        
        #endregion
    }
} 