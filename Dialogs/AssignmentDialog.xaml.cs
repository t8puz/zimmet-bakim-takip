using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Dialogs;

namespace Zimmet_Bakim_Takip.Dialogs
{
    public partial class AssignmentDialog : Window
    {
        private Assignment _assignment;
        private bool _isEditMode;
        private List<Device> _deviceList;
        private List<Personnel> _personnelList;

        // Yeni zimmet oluşturma modu
        public AssignmentDialog(List<Device> devices, List<Personnel> personnel)
        {
            InitializeComponent();
            
            _isEditMode = false;
            _assignment = new Assignment();
            _deviceList = devices;
            _personnelList = personnel;
            
            LoadComboBoxes();
            SetupDefaultValues();
        }

        // Mevcut zimmeti düzenleme modu
        public AssignmentDialog(Assignment assignment, List<Device> devices, List<Personnel> personnel)
        {
            InitializeComponent();
            
            _isEditMode = true;
            _assignment = assignment ?? new Assignment();
            _deviceList = devices;
            _personnelList = personnel;
            
            txtTitle.Text = "Zimmet Düzenle";
            LoadComboBoxes();
            LoadAssignmentData();
        }

        public Assignment Assignment => _assignment;

        private void LoadComboBoxes()
        {
            // Cihazları yükle
            cmbDevice.ItemsSource = _deviceList;
            
            // Personeli yükle
            cmbPersonnel.ItemsSource = _personnelList;
        }

        private void SetupDefaultValues()
        {
            // Varsayılan değerleri ayarla
            dpAssignmentDate.SelectedDate = DateTime.Today;
            cmbStatus.SelectedIndex = 0; // Aktif
        }

        private void LoadAssignmentData()
        {
            // Mevcut zimmet verilerini yükle
            if (_assignment.DeviceId > 0)
            {
                var device = _deviceList.FirstOrDefault(d => d.Id == _assignment.DeviceId);
                if (device != null)
                {
                    cmbDevice.SelectedItem = device;
                }
            }

            if (_assignment.PersonnelId > 0)
            {
                var personnel = _personnelList.FirstOrDefault(p => p.Id == _assignment.PersonnelId);
                if (personnel != null)
                {
                    cmbPersonnel.SelectedItem = personnel;
                }
            }

            txtDepartment.Text = _assignment.Department;
            dpAssignmentDate.SelectedDate = _assignment.AssignmentDate;
            dpReturnDate.SelectedDate = _assignment.ReturnDate;
            txtNotes.Text = _assignment.Notes;

            // Durum seçimi
            switch (_assignment.Status)
            {
                case "Aktif":
                    cmbStatus.SelectedIndex = 0;
                    break;
                case "Tamamlandı":
                    cmbStatus.SelectedIndex = 1;
                    break;
                case "İptal Edildi":
                    cmbStatus.SelectedIndex = 2;
                    break;
                default:
                    cmbStatus.SelectedIndex = 0;
                    break;
            }
        }

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
                    _deviceList.Add(newDevice);
                    
                    // ComboBox'ı güncelle
                    cmbDevice.ItemsSource = null;
                    cmbDevice.ItemsSource = _deviceList;
                    
                    // Yeni eklenen cihazı seç
                    cmbDevice.SelectedItem = newDevice;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cihaz eklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void btnAddPersonnel_Click(object sender, RoutedEventArgs e)
        {
            var personnelDialog = new PersonnelEditDialog();
            personnelDialog.Owner = this;

            if (personnelDialog.ShowDialog() == true)
            {
                try
                {
                    // Yeni personeli veritabanına ekle
                    var newPersonnel = personnelDialog.Personnel;
                    
                    // Personelin ID'sini oluşturmak için servis çağrısı
                    if (newPersonnel.Id == 0)
                    {
                        await App.PersonelService.AddAsync(newPersonnel);
                    }
                    
                    // Personel listesini güncelle
                    _personnelList.Add(newPersonnel);
                    
                    // ComboBox'ı güncelle
                    cmbPersonnel.ItemsSource = null;
                    cmbPersonnel.ItemsSource = _personnelList;
                    
                    // Yeni eklenen personeli seç
                    cmbPersonnel.SelectedItem = newPersonnel;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Personel eklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // Form verilerini nesneye aktar
                if (cmbDevice.SelectedItem is Device selectedDevice)
                {
                    _assignment.DeviceId = selectedDevice.Id;
                    _assignment.Device = selectedDevice;
                }

                if (cmbPersonnel.SelectedItem is Personnel selectedPersonnel)
                {
                    _assignment.PersonnelId = selectedPersonnel.Id;
                    _assignment.Personnel = selectedPersonnel;
                }

                // Kullanıcı adı alanı kaldırıldı, boş değer atayalım
                _assignment.UserName = string.Empty;
                _assignment.Department = txtDepartment.Text.Trim();
                _assignment.AssignmentDate = dpAssignmentDate.SelectedDate ?? DateTime.Now;
                _assignment.ReturnDate = dpReturnDate.SelectedDate;
                _assignment.Notes = txtNotes.Text.Trim();

                if (cmbStatus.SelectedItem is ComboBoxItem selectedStatus)
                {
                    _assignment.Status = selectedStatus.Content.ToString() ?? "Aktif";
                }

                // Sistem bilgilerini güncelle
                _assignment.UpdatedAt = DateTime.Now;
                _assignment.UpdatedBy = Environment.UserName;

                if (!_isEditMode)
                {
                    _assignment.CreatedAt = DateTime.Now;
                    _assignment.CreatedBy = Environment.UserName;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Zimmet kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Personel seçimi kontrolü
            if (cmbPersonnel.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir personel seçin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbPersonnel.Focus();
                return false;
            }

            // Zimmet tarihi kontrolü
            if (!dpAssignmentDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Lütfen zimmet tarihini seçin.", "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpAssignmentDate.Focus();
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
    }
} 