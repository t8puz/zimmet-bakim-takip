using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Dialogs;
using System.Globalization;
using System.Diagnostics;

namespace Zimmet_Bakim_Takip.Dialogs
{
    public partial class MaintenanceRecordDialog : Window
    {
        private MaintenanceRecord _record = new MaintenanceRecord();
        private bool _isEditMode;
        private List<Device> _devicesList;
        private Maintenance _maintenance;
        
        // Bakım türleri listesi
        private List<string> _maintenanceTypes = new List<string>
        {
            "Genel Bakım",
            "Periyodik Bakım",
            "Arıza Giderme",
            "Yazılım Güncelleme",
            "Donanım Değişimi",
            "Kalibrasyon"
        };
        
        // Durum listesi
        private List<string> _statusList = new List<string>
        {
            "Planlandı",
            "Devam Ediyor",
            "Tamamlandı",
            "İptal Edildi",
            "Ertelendi"
        };
        
        public MaintenanceRecordDialog(MaintenanceRecord? record, List<Device>? devices, List<string> maintenanceTypes)
        {
            InitializeComponent();
            
            // Listeler
            _devicesList = devices ?? new List<Device>();
            cmbDevice.ItemsSource = _devicesList;
            cmbMaintenanceType.ItemsSource = maintenanceTypes;
            
            _isEditMode = record != null;
            
            if (_isEditMode)
            {
                // Düzenleme modu
                _record = record ?? new MaintenanceRecord();
                txtTitle.Text = "Bakım Kaydını Düzenle";
                LoadRecordData();
            }
            else
            {
                // Yeni kayıt modu
                _record = new MaintenanceRecord
                {
                    MaintenanceDate = DateTime.Today,
                    NextMaintenanceDate = DateTime.Today.AddMonths(3),
                    Status = "Planlandı",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = Environment.UserName
                };
                
                // Varsayılan değerleri ayarla
                dpMaintenanceDate.SelectedDate = DateTime.Today;
                dpNextMaintenanceDate.SelectedDate = DateTime.Today.AddMonths(3);
                SetComboBoxItemByContent(cmbStatus, "Planlandı");
            }
        }
        
        public MaintenanceRecordDialog(Maintenance maintenance)
        {
            InitializeComponent();
            Debug.WriteLine("MaintenanceRecordDialog(Maintenance) constructor başladı");
            
            _isEditMode = true;
            _maintenance = maintenance;
            
            // Tarih kontrollerinin formatını ayarla
            SetupDateFormat();
            
            // Başlık güncelleniyor
            txtTitle.Text = "Bakım Kaydı Düzenle";
            
            // Bakım türlerini yükle
            LoadMaintenanceTypes();
            
            // Cihazları yükle
            LoadDevices();
            
            // Form verilerini doldur
            FillFormData();
            
            // Dosya eklerini yükle
            this.Loaded += async (s, e) => 
            {
                try
                {
                    Debug.WriteLine($"Attachments yükleniyor, maintenance.Id: {maintenance.Id}");
                    if (maintenance.Id > 0)
                    {
                        await attachmentControl.Initialize("Maintenance", maintenance.Id);
                        Debug.WriteLine("Attachments başarıyla yüklendi");
                    }
                    else
                    {
                        Debug.WriteLine("Maintenance ID geçersiz olduğu için ekler yüklenemedi");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ekler yüklenirken hata: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                    }
                    MessageBox.Show($"Dosya ekleri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            Debug.WriteLine("MaintenanceRecordDialog(Maintenance) constructor tamamlandı");
        }
        
        /// <summary>
        /// Tarih kontrollerinin formatını ayarlar
        /// </summary>
        private void SetupDateFormat()
        {
            // Tarih formatı için CultureInfo ayarla
            dpMaintenanceDate.Language = System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            dpNextMaintenanceDate.Language = System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            
            // Varsayılan değerleri ayarla
            dpMaintenanceDate.SelectedDate = DateTime.Today;
            dpNextMaintenanceDate.SelectedDate = DateTime.Today.AddMonths(3);
        }
        
        /// <summary>
        /// Bakım türlerini combo box'a yükler
        /// </summary>
        private void LoadMaintenanceTypes()
        {
            try
            {
                // MaintenanceTypes ComboBox'ını doldur
                cmbMaintenanceType.ItemsSource = _maintenanceTypes;
                cmbMaintenanceType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bakım türleri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Cihazları veritabanından yükler ve combo box'a doldurur
        /// </summary>
        private async void LoadDevices()
        {
            try
            {
                _devicesList = await App.CihazService.GetAllDevicesAsync();
                cmbDevice.ItemsSource = _devicesList;
                
                if (_devicesList.Count > 0)
                {
                    cmbDevice.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cihazlar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Cihaz ekleme butonu click event'ı
        private async void btnAddDevice_Click(object sender, RoutedEventArgs e)
        {
            var deviceDialog = new DeviceEditDialog();
            deviceDialog.Owner = this;
            
            if (deviceDialog.ShowDialog() == true)
            {
                try
                {
                    // Yeni cihazı veritabanına ekle
                    var newDevice = deviceDialog.Device;
                    
                    // Cihazın ID'sini oluşturmak için servis çağrısı
                    if (newDevice.Id == 0)
                    {
                        await App.CihazService.AddDeviceAsync(newDevice);
                    }
                    
                    // Cihaz listesini güncelle
                    _devicesList.Add(newDevice);
                    
                    // ComboBox'ı güncelle
                    cmbDevice.ItemsSource = null;
                    cmbDevice.ItemsSource = _devicesList;
                    
                    // Yeni eklenen cihazı seç
                    cmbDevice.SelectedItem = newDevice;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cihaz eklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void LoadRecordData()
        {
            try
            {
                // Cihaz seçimi
                var device = _devicesList.FirstOrDefault(d => d.Id == _record.DeviceId);
                if (device != null)
                {
                    cmbDevice.SelectedItem = device;
                }
                
                // Tarih seçimleri
                dpMaintenanceDate.SelectedDate = _record.MaintenanceDate;
                dpNextMaintenanceDate.SelectedDate = _record.NextMaintenanceDate;
                
                // Bakım tipi
                SelectMaintenanceType(_record.MaintenanceType);
                
                // Form verilerini yükle
                txtTechnicianName.Text = _record.TechnicianName;
                SetComboBoxItemByContent(cmbStatus, _record.Status);
                
                if (_record.Cost.HasValue)
                {
                    txtCost.Text = _record.Cost.Value.ToString("0.00").Replace(',', '.');
                }
                
                txtDescription.Text = _record.Description;
                txtNotes.Text = _record.Notes;
                
                // Dosya eklerini yükle (bu kısmı ekledik)
                this.Loaded += async (s, e) => 
                {
                    try
                    {
                        Debug.WriteLine($"Attachments yükleniyor, record.Id: {_record.Id}");
                        if (_record.Id > 0)
                        {
                            await attachmentControl.Initialize("Maintenance", _record.Id);
                            Debug.WriteLine("Attachments başarıyla yüklendi");
                        }
                        else
                        {
                            Debug.WriteLine("Record ID geçersiz olduğu için ekler yüklenemedi");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ekler yüklenirken hata: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                        }
                        MessageBox.Show($"Dosya ekleri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bakım kaydı verileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SetComboBoxItemByContent(System.Windows.Controls.ComboBox comboBox, string? content)
        {
            if (content == null)
                return;
                
            foreach (var item in comboBox.Items)
            {
                if (item.ToString() == content)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }
        
        /// <summary>
        /// ID'ye göre cihazı ComboBox'ta seçer
        /// </summary>
        private void SelectDeviceById(int deviceId)
        {
            if (deviceId <= 0 || cmbDevice.Items.Count == 0)
                return;
                
            foreach (Device device in cmbDevice.Items)
            {
                if (device.Id == deviceId)
                {
                    cmbDevice.SelectedItem = device;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Bakım türünü ComboBox'ta seçer
        /// </summary>
        private void SelectMaintenanceType(string maintenanceType)
        {
            if (string.IsNullOrEmpty(maintenanceType) || cmbMaintenanceType.Items.Count == 0)
                return;
                
            if (cmbMaintenanceType.Items.Contains(maintenanceType))
            {
                cmbMaintenanceType.SelectedItem = maintenanceType;
            }
            else
            {
                cmbMaintenanceType.Text = maintenanceType;
            }
        }
        
        /// <summary>
        /// Durum değerini ComboBox'ta seçer
        /// </summary>
        private void SelectStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return;
                
            foreach (var item in cmbStatus.Items)
            {
                if (item is ComboBoxItem comboBoxItem && 
                    comboBoxItem.Content != null && 
                    comboBoxItem.Content.ToString() == status)
                {
                    cmbStatus.SelectedItem = comboBoxItem;
                    return;
                }
            }
            
            // Eğer bulunamazsa ve ComboBox'ta doğrudan string değerler varsa
            if (cmbStatus.Items.Contains(status))
            {
                cmbStatus.SelectedItem = status;
            }
        }
        
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Bakım tarihi değiştiğinde sonraki bakım tarihini otomatik güncelle
            if (dpMaintenanceDate.SelectedDate.HasValue && !_isEditMode)
            {
                dpNextMaintenanceDate.SelectedDate = dpMaintenanceDate.SelectedDate.Value.AddMonths(3);
            }
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Sadece sayı ve decimal ayracına izin ver
            Regex regex = new Regex(@"^[0-9]*(?:\,[0-9]*)?$");
            string proposedText = txtCost.Text + e.Text;
            
            e.Handled = !regex.IsMatch(proposedText);
        }
        
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Form kontrolü yap
                if (!ValidateForm())
                {
                    return;
                }
                
                // Form verilerini kaydet
                SaveFormData();
                
                // MaintenanceRecord'u kaydet
                if (_isEditMode)
                {
                    bool updateSuccess = await App.MaintenanceService.UpdateMaintenanceAsync(_maintenance);
                    if (!updateSuccess)
                    {
                        MessageBox.Show("Bakım kaydı güncellenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    bool addSuccess = await App.MaintenanceService.AddMaintenanceAsync(_maintenance);
                    if (!addSuccess)
                    {
                        MessageBox.Show("Bakım kaydı eklenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                
                // Ekleri kaydet
                if (_maintenance.Id > 0)
                {
                    await attachmentControl.SaveAttachments("Maintenance", _maintenance.Id);
                }
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bakım kaydı kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private bool ValidateForm()
        {
            // Cihaz seçimi kontrolü
            if (cmbDevice.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir cihaz seçin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbDevice.Focus();
                return false;
            }
            
            // Bakım tarihi kontrolü
            if (!dpMaintenanceDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Lütfen bakım tarihini seçin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpMaintenanceDate.Focus();
                return false;
            }
            
            // Sonraki bakım tarihi kontrolü
            if (!dpNextMaintenanceDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Lütfen sonraki bakım tarihini seçin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpNextMaintenanceDate.Focus();
                return false;
            }
            
            // Bakım tipi kontrolü
            if (string.IsNullOrWhiteSpace(cmbMaintenanceType.Text))
            {
                MessageBox.Show("Lütfen bakım tipini belirtin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbMaintenanceType.Focus();
                return false;
            }
            
            // Durum kontrolü
            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir durum seçin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbStatus.Focus();
                return false;
            }
            
            return true;
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void FillFormData()
        {
            if (_maintenance == null) return;
            
            // Bakımla ilişkili cihazı bul
            SelectDeviceById(_maintenance.DeviceId);
            
            // Tarih bilgileri
            dpMaintenanceDate.SelectedDate = _maintenance.MaintenanceDate;
            dpNextMaintenanceDate.SelectedDate = _maintenance.NextMaintenanceDate;
            
            // Diğer alan bilgileri
            SelectMaintenanceType(_maintenance.MaintenanceType);
            txtTechnicianName.Text = _maintenance.Technician;
            txtDepartment.Text = _maintenance.Department;
            SelectStatus(_maintenance.Status);
            
            if (_maintenance.Cost.HasValue)
            {
                txtCost.Text = _maintenance.Cost.Value.ToString("C2").Replace("₺", "").Trim();
            }
            
            txtDescription.Text = _maintenance.Description;
            
            // Dosya ekleri için ID kontrolü
            if (_maintenance.Id <= 0)
            {
                Debug.WriteLine("Uyarı: Maintenance kaydının Id değeri 0 veya geçersiz. Dosya ekleri yüklenemeyecek.");
                MessageBox.Show("Bu bakım kaydı henüz veritabanına kaydedilmemiş. Dosya eklemek için önce kaydı oluşturun.",
                             "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void SaveFormData()
        {
            // Veri doğrulama
            if (cmbDevice.SelectedItem == null)
            {
                throw new Exception("Lütfen bir cihaz seçin.");
            }
            
            if (dpMaintenanceDate.SelectedDate == null)
            {
                throw new Exception("Lütfen bakım tarihini seçin.");
            }
            
            // Seçilen cihazı al
            var selectedDevice = cmbDevice.SelectedItem as Device;
            
            // Bakım bilgilerini güncelle/oluştur
            _maintenance.DeviceId = selectedDevice.Id;
            _maintenance.MaintenanceDate = dpMaintenanceDate.SelectedDate.Value;
            _maintenance.NextMaintenanceDate = dpNextMaintenanceDate.SelectedDate;
            _maintenance.MaintenanceType = cmbMaintenanceType.SelectedValue?.ToString() ?? "Genel Bakım";
            _maintenance.Status = cmbStatus.SelectedItem is ComboBoxItem item ? item.Content.ToString() : "Tamamlandı";
            _maintenance.Technician = txtTechnicianName.Text?.Trim() ?? string.Empty;
            _maintenance.Department = txtDepartment.Text?.Trim() ?? string.Empty;
            _maintenance.Description = txtDescription.Text?.Trim() ?? string.Empty;
            
            // Maliyet alanı
            if (decimal.TryParse(txtCost.Text, out decimal cost))
            {
                _maintenance.Cost = cost;
            }
            else
            {
                _maintenance.Cost = null;
            }
            
            // Yeni bakım kaydı ekleme
            if (!_isEditMode)
            {
                _maintenance.CreatedAt = DateTime.Now;
                _maintenance.CreatedBy = "Sistem"; // TODO: Giriş yapan kullanıcı adını al
            }
            
            // Her bakım güncellemesinde
            _maintenance.UpdatedAt = DateTime.Now;
            _maintenance.UpdatedBy = "Sistem"; // TODO: Giriş yapan kullanıcı adını al
        }
    }
} 