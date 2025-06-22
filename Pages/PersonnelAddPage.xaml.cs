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
    /// PersonnelAddPage.xaml için etkileşim mantığı
    /// </summary>
    public partial class PersonnelAddPage : Page
    {
        public PersonnelAddPage()
        {
            InitializeComponent();
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

                // Yeni personel nesnesi oluştur
                var personnel = new Personnel
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Department = txtDepartment.Text.Trim(),
                    Position = txtPosition.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    IsActive = chkIsActive.IsChecked ?? true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = "Sistem", // TODO: Giriş yapan kullanıcı adını al
                    UpdatedBy = "Sistem"
                };

                // Personel servisini kullanarak kaydet
                bool success = await App.PersonelService.AddAsync(personnel);

                if (success)
                {
                    MessageBox.Show("Personel başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Personel listesine geri dön
                    if (this.NavigationService != null)
                    {
                        this.NavigationService.GoBack();
                    }
                }
                else
                {
                    MessageBox.Show("Personel kaydedilirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Personel kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (!IsFormEmpty())
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
        /// Formun boş olup olmadığını kontrol eder
        /// </summary>
        private bool IsFormEmpty()
        {
            return string.IsNullOrWhiteSpace(txtFirstName.Text) &&
                   string.IsNullOrWhiteSpace(txtLastName.Text) &&
                   string.IsNullOrWhiteSpace(txtDepartment.Text) &&
                   string.IsNullOrWhiteSpace(txtPosition.Text) &&
                   string.IsNullOrWhiteSpace(txtEmail.Text) &&
                   string.IsNullOrWhiteSpace(txtPhone.Text) &&
                   string.IsNullOrWhiteSpace(txtAddress.Text);
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