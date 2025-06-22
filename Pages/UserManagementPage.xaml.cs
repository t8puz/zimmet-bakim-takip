using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Dialogs;

namespace Zimmet_Bakim_Takip.Pages
{
    public partial class UserManagementPage : Page
    {
        private IUserService _userService;
        private List<User> _users;
        
        public UserManagementPage()
        {
            InitializeComponent();
            
            // Kullanıcı servisini almak için App üzerinden erişim
            _userService = App.UserService;
            
            // Sayfa yüklenince kullanıcıları getir
            Loaded += async (s, e) => await LoadUsersAsync();
        }
        
        private async Task LoadUsersAsync()
        {
            try
            {
                // Veritabanından tüm kullanıcıları al
                var users = await _userService.GetAllUsersAsync();
                _users = users.ToList();
                
                // DataGrid'i doldur
                UsersDataGrid.ItemsSource = _users;
                
                // Toplam kullanıcı sayısını güncelle
                TotalUsersText.Text = $"Toplam: {_users.Count} kullanıcı";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kullanıcılar yüklenirken hata: {ex.Message}");
                MessageBox.Show($"Kullanıcılar yüklenirken bir hata oluştu: {ex.Message}", 
                               "Veri Yükleme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }
        
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Yeni kullanıcı ekleme işlemi için dialog aç
            var dialog = new UserDialog();
            
            if (dialog.ShowDialog() == true)
            {
                // Dialog kapatıldığında listeyi yenile
                _ = LoadUsersAsync();
            }
        }
        
        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Seçim değiştiğinde contextmenu durumunu güncelleyebiliriz
        }
        
        private void EditUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser != null)
            {
                EditUser(selectedUser);
            }
        }
        
        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            // Tıklanan butondan kullanıcı ID'sini al
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    EditUser(user);
                }
            }
        }
        
        private void EditUser(User user)
        {
            // Kullanıcı düzenleme işlemi için dialog aç
            var dialog = new UserDialog(user);
            
            if (dialog.ShowDialog() == true)
            {
                // Dialog kapatıldığında listeyi yenile
                _ = LoadUsersAsync();
            }
        }
        
        private void ResetPasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser != null)
            {
                ResetPassword(selectedUser);
            }
        }
        
        private async void ResetPassword(User user)
        {
            // Şifre sıfırlama dialog'u göster
            var dialog = new PasswordResetDialog(user);
            
            if (dialog.ShowDialog() == true)
            {
                // Dialog kapatıldığında listeyi yenileyebiliriz (şifre değişirse)
                await LoadUsersAsync();
            }
        }
        
        private void DeleteUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser != null)
            {
                DeleteUser(selectedUser);
            }
        }
        
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            // Tıklanan butondan kullanıcı ID'sini al
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    DeleteUser(user);
                }
            }
        }
        
        private async void DeleteUser(User user)
        {
            try
            {
                // Eğer bu kişi oturum açmış kullanıcı ise (kendinizi silemezsiniz)
                if (LoginWindow.CurrentUser != null && LoginWindow.CurrentUser.Id == user.Id)
                {
                    MessageBox.Show("Kendinizi silemezsiniz!", "İşlem Engellendi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Silme onayı
                var result = MessageBox.Show(
                    $"{user.Username} kullanıcısını silmek istediğinize emin misiniz?",
                    "Kullanıcı Silme Onayı", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Kullanıcıyı sil
                    var success = await _userService.DeleteUserAsync(user.Id);
                    
                    if (success)
                    {
                        MessageBox.Show("Kullanıcı başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsersAsync(); // Listeyi yenile
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı silinirken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kullanıcı silme hatası: {ex.Message}");
                MessageBox.Show($"Kullanıcı silinirken bir hata oluştu: {ex.Message}", 
                               "Silme Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 