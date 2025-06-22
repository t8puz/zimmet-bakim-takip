using System;
using System.Windows;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;
using System.Diagnostics;

namespace Zimmet_Bakim_Takip.Dialogs
{
    public partial class PasswordResetDialog : Window
    {
        private readonly IUserService _userService;
        private readonly User _user;
        
        public PasswordResetDialog(User user)
        {
            InitializeComponent();
            
            _userService = App.UserService;
            _user = user;
            
            // Kullanıcı bilgilerini göster
            UserNameText.Text = $"{user.FullName} ({user.Username})";
        }
        
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Şifre alanlarını kontrol et
                if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
                {
                    MessageBox.Show("Yeni şifre boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    NewPasswordBox.Focus();
                    return;
                }
                
                if (NewPasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Şifre en az 6 karakter olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    NewPasswordBox.Focus();
                    return;
                }
                
                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    ConfirmPasswordBox.Focus();
                    return;
                }
                
                // Şifre sıfırlama onayı
                var result = MessageBox.Show(
                    $"{_user.Username} kullanıcısının şifresini sıfırlamak istediğinize emin misiniz?",
                    "Şifre Sıfırlama Onayı", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Şifreyi sıfırla
                    var success = await _userService.ResetPasswordAsync(_user.Id, NewPasswordBox.Password);
                    
                    if (success)
                    {
                        MessageBox.Show("Şifre başarıyla sıfırlandı.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Şifre sıfırlanırken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Şifre sıfırlama hatası: {ex.Message}");
                MessageBox.Show($"Şifre sıfırlanırken bir hata oluştu: {ex.Message}", 
                               "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 