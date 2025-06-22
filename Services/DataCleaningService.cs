using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Zimmet_Bakim_Takip.Services
{
    /// <summary>
    /// Uygulamadaki örnek/test verilerini temizlemek için servis
    /// </summary>
    public class DataCleaningService
    {
        private readonly AppDbContext _context;

        public DataCleaningService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm örnek verileri temizler (admin kullanıcısı dışında)
        /// </summary>
        public async Task<(bool Success, string Message)> ClearAllSampleDataAsync()
        {
            try
            {
                Debug.WriteLine("Örnek veri temizleme işlemi başlatılıyor...");

                // Önce yedek oluştur
                var backupResult = await App.DataBackupService.BackupDatabaseAsync();
                
                if (!backupResult.Success)
                {
                    return (false, "Güvenlik için yedek alınamadı. İşlem iptal edildi.");
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Attachments (Ekler) tablosunu temizle
                        var attachments = await _context.Attachments.ToListAsync();
                        _context.Attachments.RemoveRange(attachments);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{attachments.Count} adet ek dosyası kaydı silindi.");

                        // 2. MaintenanceRecords (Bakım Kayıtları) tablosunu temizle
                        var maintenanceRecords = await _context.MaintenanceRecords.ToListAsync();
                        _context.MaintenanceRecords.RemoveRange(maintenanceRecords);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{maintenanceRecords.Count} adet bakım kaydı silindi.");

                        // 3. Maintenances (Bakımlar) tablosunu temizle
                        var maintenances = await _context.Maintenances.ToListAsync();
                        _context.Maintenances.RemoveRange(maintenances);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{maintenances.Count} adet bakım silindi.");

                        // 4. Bakimlar tablosunu temizle
                        var bakimlar = await _context.Bakimlar.ToListAsync();
                        _context.Bakimlar.RemoveRange(bakimlar);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{bakimlar.Count} adet bakım silindi.");

                        // 5. Assignments (Zimmetler) tablosunu temizle
                        var assignments = await _context.Assignments.ToListAsync();
                        _context.Assignments.RemoveRange(assignments);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{assignments.Count} adet zimmet kaydı silindi.");

                        // 6. Devices (Cihazlar) tablosunu temizle
                        var devices = await _context.Devices.ToListAsync();
                        _context.Devices.RemoveRange(devices);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{devices.Count} adet cihaz silindi.");

                        // 7. Personnel (Personel) tablosunu temizle
                        var personnel = await _context.Personnel.ToListAsync();
                        _context.Personnel.RemoveRange(personnel);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{personnel.Count} adet personel kaydı silindi.");

                        // 8. Admin dışındaki kullanıcıları temizle
                        var usersToDelete = await _context.Users
                            .Where(u => u.Username != "admin")
                            .ToListAsync();
                        _context.Users.RemoveRange(usersToDelete);
                        await _context.SaveChangesAsync();
                        Debug.WriteLine($"{usersToDelete.Count} adet kullanıcı silindi (admin korundu).");

                        // Transaction'ı onayla
                        await transaction.CommitAsync();

                        // Ek dosyalarını fiziksel olarak temizle
                        await CleanupAttachmentFilesAsync();

                        Debug.WriteLine("Örnek veri temizleme işlemi başarıyla tamamlandı.");
                        return (true, $"Tüm örnek veriler başarıyla temizlendi.\nYedek dosyası: {backupResult.FilePath}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Debug.WriteLine($"Temizleme işlemi sırasında hata: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Veri temizleme hatası: {ex.Message}");
                return (false, $"Veri temizleme sırasında hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Sadece belirli veri türlerini temizler
        /// </summary>
        public async Task<(bool Success, string Message)> ClearSpecificDataAsync(
            bool clearDevices = true,
            bool clearPersonnel = true,
            bool clearAssignments = true,
            bool clearMaintenances = true,
            bool clearAttachments = true,
            bool clearUsers = false)
        {
            try
            {
                Debug.WriteLine("Seçili veri türleri temizleniyor...");

                // Önce yedek oluştur
                var backupResult = await App.DataBackupService.BackupDatabaseAsync();

                if (!backupResult.Success)
                {
                    return (false, "Güvenlik için yedek alınamadı. İşlem iptal edildi.");
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var deletedCounts = new System.Collections.Generic.Dictionary<string, int>();

                        if (clearAttachments)
                        {
                            var items = await _context.Attachments.ToListAsync();
                            _context.Attachments.RemoveRange(items);
                            deletedCounts["Ekler"] = items.Count;
                        }

                        if (clearMaintenances)
                        {
                            var maintenanceRecords = await _context.MaintenanceRecords.ToListAsync();
                            _context.MaintenanceRecords.RemoveRange(maintenanceRecords);
                            deletedCounts["Bakım Kayıtları"] = maintenanceRecords.Count;

                            var maintenances = await _context.Maintenances.ToListAsync();
                            _context.Maintenances.RemoveRange(maintenances);
                            deletedCounts["Bakımlar"] = maintenances.Count;

                            var bakimlar = await _context.Bakimlar.ToListAsync();
                            _context.Bakimlar.RemoveRange(bakimlar);
                            deletedCounts["Bakımlar (Eski)"] = bakimlar.Count;
                        }

                        if (clearAssignments)
                        {
                            var items = await _context.Assignments.ToListAsync();
                            _context.Assignments.RemoveRange(items);
                            deletedCounts["Zimmetler"] = items.Count;
                        }

                        if (clearDevices)
                        {
                            var items = await _context.Devices.ToListAsync();
                            _context.Devices.RemoveRange(items);
                            deletedCounts["Cihazlar"] = items.Count;
                        }

                        if (clearPersonnel)
                        {
                            var items = await _context.Personnel.ToListAsync();
                            _context.Personnel.RemoveRange(items);
                            deletedCounts["Personel"] = items.Count;
                        }

                        if (clearUsers)
                        {
                            var usersToDelete = await _context.Users
                                .Where(u => u.Username != "admin")
                                .ToListAsync();
                            _context.Users.RemoveRange(usersToDelete);
                            deletedCounts["Kullanıcılar (admin hariç)"] = usersToDelete.Count;
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        if (clearAttachments)
                        {
                            await CleanupAttachmentFilesAsync();
                        }

                        var resultMessage = "Seçili veriler başarıyla temizlendi:\n" +
                            string.Join("\n", deletedCounts.Select(kv => $"- {kv.Key}: {kv.Value} adet")) +
                            $"\n\nYedek dosyası: {backupResult.FilePath}";

                        Debug.WriteLine("Seçili veri temizleme işlemi başarıyla tamamlandı.");
                        return (true, resultMessage);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Debug.WriteLine($"Temizleme işlemi sırasında hata: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Veri temizleme hatası: {ex.Message}");
                return (false, $"Veri temizleme sırasında hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Ek dosyalarını fiziksel olarak temizler
        /// </summary>
        private async Task CleanupAttachmentFilesAsync()
        {
            try
            {
                var attachmentFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
                
                if (Directory.Exists(attachmentFolder))
                {
                    var files = Directory.GetFiles(attachmentFolder);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            Debug.WriteLine($"Ek dosyası silindi: {Path.GetFileName(file)}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ek dosyası silinemedi {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                    Debug.WriteLine($"{files.Length} adet ek dosyası temizlendi.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ek dosyalar temizlenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Veritabanında toplam kayıt sayılarını döndürür
        /// </summary>
        public async Task<string> GetDataSummaryAsync()
        {
            try
            {
                var deviceCount = await _context.Devices.CountAsync();
                var personnelCount = await _context.Personnel.CountAsync();
                var assignmentCount = await _context.Assignments.CountAsync();
                var maintenanceRecordCount = await _context.MaintenanceRecords.CountAsync();
                var maintenanceCount = await _context.Maintenances.CountAsync();
                var bakimCount = await _context.Bakimlar.CountAsync();
                var attachmentCount = await _context.Attachments.CountAsync();
                var userCount = await _context.Users.CountAsync();

                return $"Mevcut Veri Özeti:\n" +
                       $"- Cihazlar: {deviceCount} adet\n" +
                       $"- Personel: {personnelCount} adet\n" +
                       $"- Zimmetler: {assignmentCount} adet\n" +
                       $"- Bakım Kayıtları: {maintenanceRecordCount} adet\n" +
                       $"- Bakımlar: {maintenanceCount} adet\n" +
                       $"- Bakımlar (Eski): {bakimCount} adet\n" +
                       $"- Ekler: {attachmentCount} adet\n" +
                       $"- Kullanıcılar: {userCount} adet";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Veri özeti alınırken hata: {ex.Message}");
                return "Veri özeti alınamadı: " + ex.Message;
            }
        }
    }
} 