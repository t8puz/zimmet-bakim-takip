using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;
using System.Diagnostics;

namespace Zimmet_Bakim_Takip.Dialogs
{
    public partial class UserDialog : Window
    {
        private readonly IUserService _userService;
        private User? _existingUser;
        private bool _isEditMode;
        
        // Yeni kullanıcı ekleme
        public UserDialog()
        {
            InitializeComponent();
            
            _userService = App.UserService;
            _isEditMode = false;
            TitleText.Text = "Yeni Kullanıcı Ekle";
            
            // Şifre alanları görünür olmalı
            PasswordBox.Visibility = Visibility.Visible;
            PasswordConfirmBox.Visibility = Visibility.Visible;
            PasswordConfirmLabel.Visibility = Visibility.Visible;
        }
        
        // Mevcut kullanıcıyı düzenleme
        public UserDialog(User user)
        {
            InitializeComponent();
            
            _userService = App.UserService;
            _existingUser = user;
            _isEditMode = true;
            TitleText.Text = $"Kullanıcı Düzenle: {user.Username}";
            
            // Şifre alanları düzenleme modunda farklı davranmalı
            PasswordBox.Password = "";
            PasswordConfirmBox.Visibility = Visibility.Collapsed;
            PasswordConfirmLabel.Visibility = Visibility.Collapsed;
            
            // Mevcut kullanıcı bilgilerini form alanlarına doldur
            UsernameTextBox.Text = user.Username;
            FirstNameTextBox.Text = user.FirstName;
            LastNameTextBox.Text = user.LastName;
            EmailTextBox.Text = user.Email;
            IsActiveCheckBox.IsChecked = user.IsActive;
            
            // Rol ComboBox'ını seç
            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if (item.Tag.ToString() == user.Role)
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Temel doğrulamalar
                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    MessageBox.Show("Kullanıcı adı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    UsernameTextBox.Focus();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                {
                    MessageBox.Show("Ad boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    FirstNameTextBox.Focus();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                {
                    MessageBox.Show("Soyad boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    LastNameTextBox.Focus();
                    return;
                }
                
                // E-posta doğrulama
                if (!string.IsNullOrWhiteSpace(EmailTextBox.Text) && !IsValidEmail(EmailTextBox.Text))
                {
                    MessageBox.Show("Geçerli bir e-posta adresi giriniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    EmailTextBox.Focus();
                    return;
                }
                
                // Şifre doğrulama (yeni kullanıcı ekleme modunda)
                if (!_isEditMode)
                {
                    if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        MessageBox.Show("Şifre boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        PasswordBox.Focus();
                        return;
                    }
                    
                    if (PasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        PasswordBox.Focus();
                        return;
                    }
                    
                    if (PasswordBox.Password != PasswordConfirmBox.Password)
                    {
                        MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        PasswordConfirmBox.Focus();
                        return;
                    }
                }
                
                // Rol seçilmiş mi kontrol et
                if (RoleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir rol seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    RoleComboBox.Focus();
                    return;
                }
                
                // Seçilen rolü al
                string role = ((ComboBoxItem)RoleComboBox.SelectedItem).Tag.ToString();
                
                if (_isEditMode && _existingUser != null)
                {
                    // Mevcut kullanıcıyı güncelle
                    _existingUser.Username = UsernameTextBox.Text;
                    _existingUser.FirstName = FirstNameTextBox.Text;
                    _existingUser.LastName = LastNameTextBox.Text;
                    _existingUser.Email = EmailTextBox.Text;
                    _existingUser.Role = role;
                    _existingUser.IsActive = IsActiveCheckBox.IsChecked ?? true;
                    
                    string password = PasswordBox.Password;
                    bool success = await _userService.UpdateUserAsync(_existingUser, 
                                                                    string.IsNullOrWhiteSpace(password) ? null : password);
                    
                    if (success)
                    {
                        MessageBox.Show("Kullanıcı başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı güncellenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Yeni kullanıcı ekleme
                    var newUser = new User
                    {
                        Username = UsernameTextBox.Text,
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text,
                        Email = EmailTextBox.Text,
                        Role = role,
                        IsActive = IsActiveCheckBox.IsChecked ?? true,
                        CreatedAt = DateTime.Now
                    };
                    
                    bool success = await _userService.AddUserAsync(newUser, PasswordBox.Password);
                    
                    if (success)
                    {
                        MessageBox.Show("Kullanıcı başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı eklenirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(argEx.Message, "Doğrulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kullanıcı kaydetme hatası: {ex.Message}");
                MessageBox.Show($"Kullanıcı kaydedilirken bir hata oluştu: {ex.Message}", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        // E-posta doğrulama için yardımcı metod
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
                
            try
            {
                // Basit e-posta doğrulama regex'i
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
} 