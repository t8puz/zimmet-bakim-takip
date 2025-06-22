using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Zimmet_Bakim_Takip.Models;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// PersonnelEditPage.xaml için etkileşim mantığı
    /// </summary>
    public partial class PersonnelEditPage : Page
    {
        private int _personnelId;
        private Personnel _personnel;
        
        public PersonnelEditPage(int personnelId)
        {
            InitializeComponent();
            _personnelId = personnelId;
            
            // Sayfa yüklendiğinde personel verilerini çek
            Loaded += PersonnelEditPage_Loaded;
        }
        
        private async void PersonnelEditPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPersonnelData();
        }
        
        /// <summary>
        /// Personel verilerini yükler
        /// </summary>
        private async Task LoadPersonnelData()
        {
            try
            {
                // Personel verisini çek
                _personnel = await App.PersonelService.GetByIdAsync(_personnelId);
                
                if (_personnel == null)
                {
                    MessageBox.Show("Personel bilgisi bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (NavigationService != null)
                    {
                        NavigationService.GoBack();
                    }
                    return;
                }
                
                // Form alanlarına verileri doldur
                txtFirstName.Text = _personnel.FirstName;
                txtLastName.Text = _personnel.LastName;
                txtDepartment.Text = _personnel.Department;
                txtPosition.Text = _personnel.Position;
                txtEmail.Text = _personnel.Email;
                txtPhone.Text = _personnel.Phone;
                txtAddress.Text = _personnel.Address;
                chkIsActive.IsChecked = _personnel.IsActive;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel bilgileri yüklenirken bir hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                if (NavigationService != null)
                {
                    NavigationService.GoBack();
                }
            }
        }

        /// <summary>
        /// Kaydet butonuna tıklandığında çalışır
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Kullanıcı arayüzünü devre dışı bırak
            SetFormEnabled(false);

            try
            {
                // Form girişlerini kontrol et
                if (!ValidateForm())
                {
                    return;
                }

                // Personel nesnesini güncelle
                _personnel.FirstName = txtFirstName.Text.Trim();
                _personnel.LastName = txtLastName.Text.Trim();
                _personnel.Department = txtDepartment.Text.Trim();
                _personnel.Position = txtPosition.Text.Trim();
                _personnel.Email = txtEmail.Text.Trim();
                _personnel.Phone = txtPhone.Text.Trim();
                _personnel.Address = txtAddress.Text.Trim();
                _personnel.IsActive = chkIsActive.IsChecked ?? true;
                _personnel.UpdatedAt = DateTime.Now;
                _personnel.UpdatedBy = "Sistem"; // TODO: Giriş yapan kullanıcı adını al

                // Personel servisini kullanarak güncelle
                bool success = await App.PersonelService.UpdateAsync(_personnel);

                if (success)
                {
                    MessageBox.Show("Personel başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Personel listesine geri dön
                    if (this.NavigationService != null)
                    {
                        this.NavigationService.GoBack();
                    }
                }
                else
                {
                    MessageBox.Show("Personel güncellenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel güncellenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Kullanıcı arayüzünü tekrar aktif et
                SetFormEnabled(true);
            }
        }

        /// <summary>
        /// İptal butonuna tıklandığında çalışır
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Kullanıcıya sor - sadece değişiklik yapıldıysa
            if (IsFormChanged())
            {
                var result = MessageBox.Show("Yaptığınız değişiklikler kaydedilmeyecek. Devam etmek istiyor musunuz?", 
                                           "Uyarı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            
            // Önceki sayfaya dön
            if (this.NavigationService != null)
            {
                this.NavigationService.GoBack();
            }
        }
        
        /// <summary>
        /// Form değerlerinin orijinal verilerden farklı olup olmadığını kontrol eder
        /// </summary>
        private bool IsFormChanged()
        {
            if (_personnel == null) return false;
            
            return txtFirstName.Text != _personnel.FirstName ||
                   txtLastName.Text != _personnel.LastName ||
                   txtDepartment.Text != _personnel.Department ||
                   txtPosition.Text != _personnel.Position ||
                   txtEmail.Text != _personnel.Email ||
                   txtPhone.Text != _personnel.Phone ||
                   txtAddress.Text != _personnel.Address ||
                   chkIsActive.IsChecked != _personnel.IsActive;
        }

        /// <summary>
        /// Form girdilerini doğrular
        /// </summary>
        private bool ValidateForm()
        {
            // Zorunlu alanları kontrol et
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Ad alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFirstName.Focus();
                SetFormEnabled(true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Soyad alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                SetFormEnabled(true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPosition.Text))
            {
                MessageBox.Show("Pozisyon alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPosition.Focus();
                SetFormEnabled(true);
                return false;
            }

            // E-posta formatını kontrol et (doldurulduysa)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Geçerli bir e-posta adresi giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                SetFormEnabled(true);
                return false;
            }

            // Telefon formatını kontrol et (doldurulduysa)
            if (!string.IsNullOrWhiteSpace(txtPhone.Text) && !IsValidPhone(txtPhone.Text))
            {
                MessageBox.Show("Geçerli bir telefon numarası giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                SetFormEnabled(true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// E-posta formatının geçerli olup olmadığını kontrol eder
        /// </summary>
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

        /// <summary>
        /// Telefon formatının geçerli olup olmadığını kontrol eder
        /// </summary>
        private bool IsValidPhone(string phone)
        {
            // Sadece sayılar, kısa çizgi, parantez ve boşluklara izin ver
            // Minimum 10, maksimum 15 karakter olabilir
            string pattern = @"^[\d\s\-\(\)]{10,15}$";
            return Regex.IsMatch(phone, pattern);
        }

        /// <summary>
        /// Formu etkinleştirir veya devre dışı bırakır
        /// </summary>
        private void SetFormEnabled(bool enabled)
        {
            txtFirstName.IsEnabled = enabled;
            txtLastName.IsEnabled = enabled;
            txtDepartment.IsEnabled = enabled;
            txtPosition.IsEnabled = enabled;
            txtEmail.IsEnabled = enabled;
            txtPhone.IsEnabled = enabled;
            txtAddress.IsEnabled = enabled;
            chkIsActive.IsEnabled = enabled;
        }
    }
} 