using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly AppDbContext _context;
        private readonly string _attachmentsFolder;

        public AttachmentService(AppDbContext context)
        {
            _context = context;
            
            // Uygulama klasöründe bir "Attachments" klasörü oluştur
            _attachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
            
            if (!Directory.Exists(_attachmentsFolder))
            {
                try
                {
                    Directory.CreateDirectory(_attachmentsFolder);
                    Debug.WriteLine($"Attachments klasörü oluşturuldu: {_attachmentsFolder}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Attachments klasörü oluşturulamadı: {ex.Message}");
                    throw new Exception($"Dosya eki klasörü oluşturulamadı: {ex.Message}", ex);
                }
            }
        }

        public async Task<List<Attachment>> GetAttachmentsAsync(string entityType, int entityId)
        {
            return await _context.Attachments
                .Where(a => a.RelatedEntityType == entityType && a.RelatedEntityId == entityId)
                .ToListAsync();
        }

        public async Task<Attachment> GetByIdAsync(int id)
        {
            return await _context.Attachments.FindAsync(id);
        }

        public async Task<Attachment> AddFromFileDialogAsync(string entityType, int entityId, string description = "")
        {
            // Dosya seçme diyaloğunu aç
            var dialog = CreateOpenFileDialog();
            
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                return await AddFromPathAsync(entityType, entityId, filePath, description);
            }
            
            return null;
        }

        public async Task<Attachment> AddFromPathAsync(string entityType, int entityId, string filePath, string description = "")
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"Dosya bulunamadı: {filePath}");
                    throw new FileNotFoundException("Dosya bulunamadı", filePath);
                }

                // Dosya bilgilerini al
                var fileInfo = new FileInfo(filePath);
                string fileName = fileInfo.Name;
                string contentType = GetContentType(fileName);
                long fileSize = fileInfo.Length;
                
                Debug.WriteLine($"Dosya bilgileri alındı: {fileName}, {contentType}, {fileSize} bytes");
                
                // Hedef klasör varlığını kontrol et
                if (!Directory.Exists(_attachmentsFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(_attachmentsFolder);
                        Debug.WriteLine($"Attachments klasörü oluşturuldu: {_attachmentsFolder}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Attachments klasörü oluşturulamadı: {ex.Message}");
                        throw new Exception($"Dosya eki klasörü oluşturulamadı: {ex.Message}", ex);
                    }
                }
                
                // Dosyayı benzersiz bir isimle attachments klasörüne kopyala
                string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}_{fileName}";
                string targetPath = Path.Combine(_attachmentsFolder, uniqueFileName);
                
                Debug.WriteLine($"Dosya kopyalanıyor: {filePath} -> {targetPath}");
                
                try
                {
                    File.Copy(filePath, targetPath);
                    Debug.WriteLine("Dosya başarıyla kopyalandı");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Dosya kopyalama hatası: {ex.Message}");
                    throw new Exception($"Dosya kopyalanırken hata oluştu: {ex.Message}", ex);
                }
                
                // Veritabanına kaydet
                var attachment = new Attachment
                {
                    FileName = fileName,
                    FilePath = uniqueFileName,
                    ContentType = contentType,
                    FileSize = fileSize,
                    RelatedEntityType = entityType,
                    RelatedEntityId = entityId,
                    Description = description,
                    UploadDate = DateTime.Now,
                    UploadedBy = Environment.UserName
                };
                
                Debug.WriteLine("Attachment nesnesi oluşturuldu, veritabanına ekleniyor");
                
                try
                {
                    await _context.Attachments.AddAsync(attachment);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine($"Ek başarıyla veritabanına kaydedildi, ID: {attachment.Id}");
                    return attachment;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Veritabanı kayıt hatası: {ex.Message}");
                    
                    // Dosyayı temizle
                    if (File.Exists(targetPath))
                    {
                        try
                        {
                            File.Delete(targetPath);
                            Debug.WriteLine("Kopyalanan dosya silindi");
                        }
                        catch
                        {
                            Debug.WriteLine("Kopyalanan dosya silinemedi");
                        }
                    }
                    
                    // İç istisnayı ekleyerek hatayı yeniden fırlat
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                        throw new Exception($"Dosya eki veritabanına kaydedilemedi: {ex.Message}, İç Hata: {ex.InnerException.Message}", ex);
                    }
                    else
                    {
                        throw new Exception($"Dosya eki veritabanına kaydedilemedi: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dosya eklenirken hata oluştu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var attachment = await _context.Attachments.FindAsync(id);
                if (attachment == null)
                {
                    return false;
                }
                
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ek silinirken hata oluştu: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteWithFileAsync(int id)
        {
            try
            {
                var attachment = await _context.Attachments.FindAsync(id);
                if (attachment == null)
                {
                    return false;
                }
                
                // Fiziksel dosyayı sil
                string filePath = Path.Combine(_attachmentsFolder, attachment.FilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                // Veritabanı kaydını sil
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ek ve dosyası silinirken hata oluştu: {ex.Message}");
                return false;
            }
        }

        public bool OpenAttachment(Attachment attachment)
        {
            try
            {
                string filePath = Path.Combine(_attachmentsFolder, attachment.FilePath);
                if (File.Exists(filePath))
                {
                    // Dosyayı varsayılan uygulama ile aç
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dosya açılırken hata oluştu: {ex.Message}");
                return false;
            }
        }

        public OpenFileDialog CreateOpenFileDialog(bool multiSelect = false)
        {
            return new OpenFileDialog
            {
                Multiselect = multiSelect,
                Filter = "Tüm Dosyalar|*.*|Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.gif|PDF Dosyaları|*.pdf|Word Dosyaları|*.doc;*.docx|Excel Dosyaları|*.xls;*.xlsx",
                Title = "Dosya Seç",
                CheckFileExists = true
            };
        }

        // Dosya uzantısına göre content type döndürür
        private string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
} 