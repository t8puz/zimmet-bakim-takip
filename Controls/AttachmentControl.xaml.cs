using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zimmet_Bakim_Takip.Models;
using System.Diagnostics;

namespace Zimmet_Bakim_Takip.Controls
{
    /// <summary>
    /// Dosya ekleri için görsel bileşen
    /// </summary>
    public partial class AttachmentControl : UserControl
    {
        private ObservableCollection<Attachment> _attachments = new ObservableCollection<Attachment>();
        private string _entityType;
        private int _entityId;
        
        public AttachmentControl()
        {
            InitializeComponent();
            lstAttachments.ItemsSource = _attachments;
            Debug.WriteLine("AttachmentControl oluşturuldu");
        }
        
        /// <summary>
        /// Dosya ekleri için veri kaynağını ayarlar
        /// </summary>
        public async Task Initialize(string entityType, int entityId)
        {
            try
            {
                Debug.WriteLine($"AttachmentControl.Initialize başladı. EntityType: {entityType}, EntityId: {entityId}");
                
                if (string.IsNullOrEmpty(entityType))
                {
                    throw new ArgumentException("Entity türü boş olamaz", nameof(entityType));
                }
                
                if (entityId <= 0)
                {
                    throw new ArgumentException("Entity ID geçersiz", nameof(entityId));
                }
                
                _entityType = entityType;
                _entityId = entityId;
                
                await LoadAttachments();
                
                Debug.WriteLine("AttachmentControl.Initialize tamamlandı");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AttachmentControl.Initialize hatası: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                throw;
            }
        }
        
        /// <summary>
        /// Eklentileri yeniden yükler
        /// </summary>
        public async Task LoadAttachments()
        {
            try
            {
                Debug.WriteLine($"AttachmentControl.LoadAttachments başladı. EntityType: {_entityType}, EntityId: {_entityId}");
                
                // Servis kullanarak varlığa bağlı tüm dosya eklerini al
                var attachments = await App.AttachmentService.GetAttachmentsAsync(_entityType, _entityId);
                
                // Koleksiyonu güncelle
                _attachments.Clear();
                foreach (var attachment in attachments)
                {
                    _attachments.Add(attachment);
                }
                
                Debug.WriteLine($"AttachmentControl.LoadAttachments tamamlandı. {attachments.Count} adet ek yüklendi");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AttachmentControl.LoadAttachments hatası: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                MessageBox.Show($"Dosya ekleri yüklenirken hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void btnAddAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Yeni dosya ekleme başladı");
                
                if (string.IsNullOrEmpty(_entityType) || _entityId <= 0)
                {
                    Debug.WriteLine("Geçersiz entity bilgileri. Önce Initialize çağrılmalı");
                    MessageBox.Show("Dosya eklemeden önce varlığı kaydetmeniz gerekiyor.", 
                                  "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Yeni dosya ekle
                var attachment = await App.AttachmentService.AddFromFileDialogAsync(_entityType, _entityId);
                
                if (attachment != null)
                {
                    _attachments.Add(attachment);
                    Debug.WriteLine($"Dosya başarıyla eklendi: {attachment.FileName}");
                    MessageBox.Show("Dosya başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Debug.WriteLine("Dosya ekleme iptal edildi veya başarısız oldu");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dosya ekleme hatası: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                MessageBox.Show($"Dosya eklenirken hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void OpenAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int attachmentId)
            {
                try
                {
                    // Eklentiyi bul
                    var attachment = await App.AttachmentService.GetByIdAsync(attachmentId);
                    
                    if (attachment != null)
                    {
                        // Dosyayı aç
                        bool success = App.AttachmentService.OpenAttachment(attachment);
                        
                        if (!success)
                        {
                            MessageBox.Show("Dosya açılamadı. Dosya mevcut değil veya erişim izni yok.", 
                                          "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Dosya eki bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya açılırken hata oluştu: {ex.Message}", 
                                  "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private async void DeleteAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int attachmentId)
            {
                try
                {
                    // Silme onayı
                    var result = MessageBox.Show("Bu dosya ekini silmek istediğinizden emin misiniz?", 
                                              "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // Dosyayı ve veritabanı kaydını tamamen sil
                        bool success = await App.AttachmentService.DeleteWithFileAsync(attachmentId);
                        
                        if (success)
                        {
                            // Listeden kaldır
                            var attachmentToRemove = _attachments.FirstOrDefault(a => a.Id == attachmentId);
                            if (attachmentToRemove != null)
                            {
                                _attachments.Remove(attachmentToRemove);
                            }
                            
                            MessageBox.Show("Dosya eki başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Dosya eki silinemedi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya eki silinirken hata oluştu: {ex.Message}", 
                                  "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void lstAttachments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Bu fonksiyon ileride genişletilebilir
        }
        
        /// <summary>
        /// Dosya eklerini kaydetmek için kullanılır
        /// </summary>
        /// <param name="entityType">Varlık tipi</param>
        /// <param name="entityId">Varlık ID</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        public async Task<bool> SaveAttachments(string entityType, int entityId)
        {
            try
            {
                Debug.WriteLine($"SaveAttachments başladı. EntityType: {entityType}, EntityId: {entityId}");
                
                if (string.IsNullOrEmpty(entityType))
                {
                    throw new ArgumentException("Entity türü boş olamaz", nameof(entityType));
                }
                
                if (entityId <= 0)
                {
                    throw new ArgumentException("Entity ID geçersiz", nameof(entityId));
                }
                
                // Entity bilgilerini güncelle
                _entityType = entityType;
                _entityId = entityId;
                
                // Ekleri yeniden yükle
                await LoadAttachments();
                
                Debug.WriteLine("SaveAttachments tamamlandı");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveAttachments hatası: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                MessageBox.Show($"Dosya ekleri kaydedilirken hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
} 