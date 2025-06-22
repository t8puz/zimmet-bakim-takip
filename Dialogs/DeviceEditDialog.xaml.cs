using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Dialogs
{
    /// <summary>
    /// Interaction logic for DeviceEditDialog.xaml
    /// </summary>
    public partial class DeviceEditDialog : Window
    {
        private bool _isEditMode = false;
        private Device _device;

        public Device Device => _device;

        /// <summary>
        /// Constructor - Yeni cihaz için dialog
        /// </summary>
        public DeviceEditDialog()
        {
            InitializeComponent();
            
            _device = new Device
            {
                Status = "Available",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = "Sistem", // TODO: Burada giriş yapan kullanıcı adını almalı
                UpdatedBy = "Sistem"
            };
            
            // Attachments kontrolünü başlat, ancak cihaz henüz oluşturulmadığı için bekle
        }

        /// <summary>
        /// Constructor - Mevcut cihaz düzenleme için dialog
        /// </summary>
        public DeviceEditDialog(Device device)
        {
            InitializeComponent();
            
            _isEditMode = true;
            _device = device;
            
            // UI'a verileri doldur
            LoadDeviceData();
            
            // Başlığı düzenle
            DialogTitle.Text = "Cihaz Düzenle";
            
            // Dosya eklerini yükle
            this.Loaded += async (s, e) => 
            {
                if (device.Id > 0)
                {
                    await attachmentControl.Initialize("Device", device.Id);
                }
            };
        }

        private void SetupInitialValues()
        {
            // Dialog başlığını ayarla
            DialogTitle.Text = _isEditMode ? "Cihaz Düzenle" : "Yeni Cihaz Ekle";
            
            if (_isEditMode)
            {
                // Mevcut cihaz bilgilerini form alanlarına doldur
                DeviceNameTextBox.Text = _device.Name;
                
                // Device.Type için ComboBox'ta uygun öğeyi seç
                if (!string.IsNullOrEmpty(_device.Type))
                {
                    foreach (ComboBoxItem item in DeviceTypeComboBox.Items)
                    {
                        if (item.Content.ToString() == _device.Type)
                        {
                            DeviceTypeComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    
                    // Eğer eşleşen bir öğe bulunamazsa ilk öğeyi seç
                    if (DeviceTypeComboBox.SelectedIndex == -1)
                    {
                        DeviceTypeComboBox.SelectedIndex = 0;
                    }
                }
                
                SerialNumberTextBox.Text = _device.SerialNumber;
                
                // Durumu Türkçe'ye çevir
                string turkishStatus = GetTurkishStatus(_device.Status);
                // Durum için ComboBox'ta uygun öğeyi seç
                for (int i = 0; i < StatusComboBox.Items.Count; i++)
                {
                    if (StatusComboBox.Items[i] is ComboBoxItem item && 
                        item.Content.ToString() == turkishStatus)
                    {
                        StatusComboBox.SelectedIndex = i;
                        break;
                    }
                }
                
                PurchaseDatePicker.SelectedDate = _device.PurchaseDate;
                WarrantyDatePicker.SelectedDate = _device.WarrantyExpiryDate;
                NotesTextBox.Text = _device.Notes;
            }
            else
            {
                // Yeni cihaz için varsayılan değerleri ayarla
                StatusComboBox.SelectedIndex = 0; // Müsait
                PurchaseDatePicker.SelectedDate = DateTime.Today;
            }
        }

        /// <summary>
        /// Kaydet butonuna tıklandığında çalışır
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Basit doğrulama
                if (string.IsNullOrWhiteSpace(DeviceNameTextBox.Text))
                {
                    MessageBox.Show("Lütfen cihaz adını girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DeviceNameTextBox.Focus();
                    return;
                }

                // Verileri al
                SaveFormData();
                
                // Kayıt/düzenleme tamamlandı olarak bildir
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cihaz kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// İptal butonuna tıklandığında çalışır
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Form verilerini device nesnesine aktarır
        /// </summary>
        private void SaveFormData()
        {
            // Form verilerini al
            _device.Name = DeviceNameTextBox.Text.Trim();
            _device.Type = (DeviceTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Diğer";
            _device.SerialNumber = SerialNumberTextBox.Text.Trim();
            _device.Department = DepartmentTextBox.Text.Trim();
            
            // Durum alanını dönüştür
            string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Müsait";
            if (status == "Müsait")
                _device.Status = "Available";
            else if (status == "Zimmetli")
                _device.Status = "Assigned";
            else if (status == "Bakımda")
                _device.Status = "InMaintenance";
            else if (status == "Arızalı")
                _device.Status = "Damaged";
            else
                _device.Status = status;
            
            // Tarih alanları
            _device.PurchaseDate = PurchaseDatePicker.SelectedDate;
            _device.WarrantyExpiryDate = WarrantyDatePicker.SelectedDate;
            
            // Diğer alanlar
            _device.Notes = NotesTextBox.Text.Trim();
            
            // Güncelleme zamanı
            _device.UpdatedAt = DateTime.Now;
            _device.UpdatedBy = "Sistem"; // TODO: Giriş yapan kullanıcı adını al
        }

        /// <summary>
        /// Cihaz verilerini forma doldurur
        /// </summary>
        private void LoadDeviceData()
        {
            // Temel bilgiler
            DeviceNameTextBox.Text = _device.Name;
            SerialNumberTextBox.Text = _device.SerialNumber;
            DepartmentTextBox.Text = _device.Department;
            NotesTextBox.Text = _device.Notes;
            
            // Cihaz tipi
            for (int i = 0; i < DeviceTypeComboBox.Items.Count; i++)
            {
                if ((DeviceTypeComboBox.Items[i] as ComboBoxItem)?.Content.ToString() == _device.Type)
                {
                    DeviceTypeComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Durum
            string turkishStatus = GetTurkishStatus(_device.Status);
            for (int i = 0; i < StatusComboBox.Items.Count; i++)
            {
                if (StatusComboBox.Items[i] is ComboBoxItem item && item.Content.ToString() == turkishStatus)
                {
                    StatusComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            PurchaseDatePicker.SelectedDate = _device.PurchaseDate;
            WarrantyDatePicker.SelectedDate = _device.WarrantyExpiryDate;
        }
        
        /// <summary>
        /// İngilizce durumu Türkçeye çevirir
        /// </summary>
        private string GetTurkishStatus(string status)
        {
            return status switch
            {
                "Available" => "Müsait",
                "Assigned" => "Zimmetli",
                "InMaintenance" => "Bakımda",
                "Damaged" => "Arızalı",
                "Retired" => "Emekli",
                _ => status
            };
        }
    }
} 