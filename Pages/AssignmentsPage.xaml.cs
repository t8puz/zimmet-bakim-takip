using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zimmet_Bakim_Takip.Dialogs;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;

namespace Zimmet_Bakim_Takip.Pages
{
    /// <summary>
    /// AssignmentsPage.xaml için etkileşim mantığı
    /// </summary>
    public partial class AssignmentsPage : Page
    {
        private ObservableCollection<Assignment> _assignments = new ObservableCollection<Assignment>();
        private List<Device> _devices = new List<Device>();
        private List<Personnel> _personnel = new List<Personnel>();
        private bool _isLoading = false;

        public AssignmentsPage()
        {
            InitializeComponent();
            
            // Bu sayfa ilk yüklendiğinde yapılacaklar
            Loaded += async (s, e) => await LoadAssignmentsAsync();
        }

        private async Task LoadAssignmentsAsync()
        {
            _isLoading = true;
            
            try
            {
                // Cihazları ve personeli yükle (diyalog için gerekli)
                _devices = await App.CihazService.GetAllDevicesAsync();
                _personnel = await App.PersonelService.GetAllAsync();
                
                // Zimmetleri yükle
                var assignments = await App.ZimmetService.GetAllAsync();
                
                _assignments.Clear();
                foreach (var assignment in assignments)
                {
                    _assignments.Add(assignment);
                }
                
                // ListView veri bağlantısını ayarla
                lvAssignments.ItemsSource = _assignments;
                
                // Veri yoksa bilgilendirme panelini göster
                UpdateNoDataVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Zimmet verileri yüklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }
        
        private void UpdateNoDataVisibility()
        {
            if (_assignments == null || _assignments.Count == 0)
            {
                NoDataPanel.Visibility = Visibility.Visible;
                lvAssignments.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoDataPanel.Visibility = Visibility.Collapsed;
                lvAssignments.Visibility = Visibility.Visible;
            }
        }
        
        // Arama işlemi
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoading) return;
            
            string searchText = SearchBox.Text.ToLower().Trim();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Arama kutusu boş ise tüm verileri göster
                lvAssignments.ItemsSource = _assignments;
            }
            else
            {
                // Arama metni ile filtreleme yap
                var filteredItems = _assignments
                    .Where(a => 
                        (a.Device?.Name.ToLower().Contains(searchText) ?? false) ||
                        (a.Personnel?.FullName.ToLower().Contains(searchText) ?? false) ||
                        a.UserName.ToLower().Contains(searchText) ||
                        a.Department.ToLower().Contains(searchText) ||
                        a.Status.ToLower().Contains(searchText))
                    .ToList();
                
                lvAssignments.ItemsSource = filteredItems;
            }
            
            UpdateNoDataVisibility();
        }
        
        // Yeni zimmet ekleme işlemi için kullanılacak
        private async void AddAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            
            var dialog = new AssignmentDialog(_devices, _personnel);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var newAssignment = dialog.Assignment;
                    await App.ZimmetService.AddAsync(newAssignment);
                    
                    // Zimmeti koleksiyona ekle
                    _assignments.Add(newAssignment);
                    
                    // Seçimleri temizle ve güncelle
                    lvAssignments.SelectedItem = null;
                    UpdateNoDataVisibility();
                    
                    MessageBox.Show("Zimmet başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Zimmet eklenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        // Zimmet detayları görüntüleme işlemi için kullanılacak
        private void ViewAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            
            var assignment = GetSelectedAssignment(sender);
            if (assignment == null) return;
            
            MessageBox.Show($"Zimmet Detayları:\n\n" +
                          $"Cihaz: {assignment.Device?.Name}\n" +
                          $"Personel: {assignment.Personnel?.FullName}\n" +
                          $"Kullanıcı Adı: {assignment.UserName}\n" +
                          $"Departman: {assignment.Department}\n" +
                          $"Zimmet Tarihi: {assignment.AssignmentDate:dd.MM.yyyy}\n" +
                          $"İade Tarihi: {(assignment.ReturnDate.HasValue ? assignment.ReturnDate.Value.ToString("dd.MM.yyyy") : "Belirtilmedi")}\n" +
                          $"Durum: {assignment.Status}\n" +
                          $"Notlar: {assignment.Notes}",
                          "Zimmet Detayları", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        // Zimmet düzenleme işlemi için kullanılacak
        private async void EditAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            
            var assignment = GetSelectedAssignment(sender);
            if (assignment == null) return;
            
            var dialog = new AssignmentDialog(assignment, _devices, _personnel);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var updatedAssignment = dialog.Assignment;
                    await App.ZimmetService.UpdateAsync(updatedAssignment);
                    
                    // Listeyi yenile
                    await LoadAssignmentsAsync();
                    
                    MessageBox.Show("Zimmet başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Zimmet güncellenirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        // Zimmet silme işlemi için kullanılacak
        private async void DeleteAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            
            var assignment = GetSelectedAssignment(sender);
            if (assignment == null) return;
            
            MessageBoxResult result = MessageBox.Show(
                $"{assignment.Device?.Name} cihazının {assignment.Personnel?.FullName} personeline ait zimmet kaydını silmek istediğinizden emin misiniz?",
                "Silme Onayı", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await App.ZimmetService.DeleteAsync(assignment.Id);
                    
                    // Zimmeti koleksiyondan kaldır
                    _assignments.Remove(assignment);
                    
                    // Seçimi temizle ve güncelle
                    lvAssignments.SelectedItem = null;
                    UpdateNoDataVisibility();
                    
                    MessageBox.Show("Zimmet başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Zimmet silinirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        // Seçili zimmeti alır
        private Assignment? GetSelectedAssignment(object sender)
        {
            Assignment? assignment = null;
            
            // Buton tıklaması ise butonun parentını bul
            if (sender is Button button)
            {
                var item = FindParent<ListViewItem>(button);
                if (item != null)
                {
                    assignment = item.DataContext as Assignment;
                }
            }
            // ListView seçimi ise doğrudan al
            else if (lvAssignments.SelectedItem != null)
            {
                assignment = lvAssignments.SelectedItem as Assignment;
            }
            
            if (assignment == null)
            {
                MessageBox.Show("Lütfen bir zimmet seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            return assignment;
        }
        
        // Parent kontrolü bulmak için yardımcı metot
        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            
            if (parent is T typedParent)
            {
                return typedParent;
            }
            
            return FindParent<T>(parent);
        }
        
        // Yenileme butonu tıklaması
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            await LoadAssignmentsAsync();
        }
    }
} 