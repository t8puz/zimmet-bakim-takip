using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Dialogs
{
    public partial class PersonnelEditDialog : Window
    {
        private Personnel _personnel;
        private bool _isEditMode;

        // Yeni personel oluşturma modu
        public PersonnelEditDialog()
        {
            InitializeComponent();
            _isEditMode = false;
            _personnel = new Personnel();
            SetupDefaultValues();
        }

        // Personel düzenleme modu
        public PersonnelEditDialog(Personnel personnel)
        {
            InitializeComponent();
            _isEditMode = true;
            _personnel = personnel ?? new Personnel();
            LoadPersonnelData();
        }

        public Personnel Personnel => _personnel;

        private void SetupDefaultValues()
        {
            chkIsActive.IsChecked = true;
        }

        private void LoadPersonnelData()
        {
            if (_personnel == null) return;

            // Form alanlarına personel verilerini yükle
            txtFirstName.Text = _personnel.FirstName;
            txtLastName.Text = _personnel.LastName;
            txtDepartment.Text = _personnel.Department;
            txtPosition.Text = _personnel.Position;
            txtEmail.Text = _personnel.Email;
            txtPhone.Text = _personnel.Phone;
            txtAddress.Text = _personnel.Address;
            chkIsActive.IsChecked = _personnel.IsActive;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // Form verilerini Personnel nesnesine aktar
                _personnel.FirstName = txtFirstName.Text.Trim();
                _personnel.LastName = txtLastName.Text.Trim();
                _personnel.Department = txtDepartment.Text.Trim();
                _personnel.Position = txtPosition.Text.Trim();
                _personnel.Email = txtEmail.Text.Trim();
                _personnel.Phone = txtPhone.Text.Trim();
                _personnel.Address = txtAddress.Text.Trim();
                _personnel.IsActive = chkIsActive.IsChecked ?? true;

                // Oluşturma/güncelleme bilgilerini ayarla
                _personnel.UpdatedAt = DateTime.Now;
                _personnel.UpdatedBy = Environment.UserName;

                if (!_isEditMode)
                {
                    _personnel.CreatedAt = DateTime.Now;
                    _personnel.CreatedBy = Environment.UserName;
                }

                // Dialog'u kapatırken başarılı dön
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateForm()
        {
            // Zorunlu alanları kontrol et
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Ad alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Soyad alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPosition.Text))
            {
                MessageBox.Show("Pozisyon alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPosition.Focus();
                return false;
            }

            // E-posta formatını kontrol et (doldurulduysa)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Geçerli bir e-posta adresi giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            // Telefon formatını kontrol et (doldurulduysa)
            if (!string.IsNullOrWhiteSpace(txtPhone.Text) && !IsValidPhone(txtPhone.Text))
            {
                MessageBox.Show("Geçerli bir telefon numarası giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Sadece sayılar, kısa çizgi, parantez ve boşluklara izin ver
            // Minimum 10, maksimum 15 karakter olabilir
            string pattern = @"^[\d\s\-\(\)]{10,15}$";
            return Regex.IsMatch(phone, pattern);
        }
    }
} 