using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Compression;
using Zimmet_Bakim_Takip.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Zimmet_Bakim_Takip.Services
{
    /// <summary>
    /// Veri yedekleme ve geri yükleme işlemleri için servis
    /// </summary>
    public class DataBackupService : IDataBackupService
    {
        private const string BACKUP_FOLDER_NAME = "Backups";
        private const string BACKUP_FILE_PREFIX = "ZimmetBakim_Backup_";
        private const string BACKUP_FILE_EXTENSION = ".zbkp";
        
        private readonly AppDbContext _context;
        
        public DataBackupService(AppDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Veritabanını belirtilen dosya konumuna yedekler
        /// </summary>
        public async Task<(bool Success, string FilePath, string Message)> BackupDatabaseAsync(string backupPath = null)
        {
            try
            {
                // Veritabanı yolunu al
                string dbPath = AppDbContextFactory.GetDbPath();
                
                if (!File.Exists(dbPath))
                {
                    return (false, string.Empty, "Veritabanı dosyası bulunamadı.");
                }
                
                // Yedekleme klasörünü belirle
                string backupFolder = GetBackupFolder();
                
                // Yedekleme klasörü yoksa oluştur
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }
                
                // Yedek dosya adını otomatik oluştur
                if (string.IsNullOrEmpty(backupPath))
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    backupPath = Path.Combine(backupFolder, $"{BACKUP_FILE_PREFIX}{timestamp}{BACKUP_FILE_EXTENSION}");
                }
                
                // Geçici dosya yolu oluştur
                string tempDbPath = Path.Combine(Path.GetTempPath(), $"temp_db_{Guid.NewGuid()}.db");
                
                try
                {
                    // Veritabanı bağlantısını kapat - bu önemli
                    await _context.Database.CloseConnectionAsync();
                    
                    // SQLite'ın dosyayı serbest bırakması için biraz bekle
                    await Task.Delay(1000);
                    
                    // En yüksek paylaşım modunda dosyayı açmaya çalış
                    int retryCount = 0;
                    bool copySuccess = false;
                    
                    while (!copySuccess && retryCount < 5)
                    {
                        try
                        {
                            // Dosyayı ReadShare modunda açmayı dene - bu diğer uygulamalara da okuma izni verir
                            using (FileStream sourceStream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (FileStream tempStream = new FileStream(tempDbPath, FileMode.Create, FileAccess.Write))
                            {
                                await sourceStream.CopyToAsync(tempStream);
                                await tempStream.FlushAsync();
                            }
                            copySuccess = true;
                        }
                        catch (IOException ex)
                        {
                            // Dosya kilitli olabilir, yeniden dene
                            retryCount++;
                            Debug.WriteLine($"Veritabanı kopyalama hatası (deneme {retryCount}): {ex.Message}");
                            
                            if (retryCount >= 5)
                                throw;  // Tüm denemelerin başarısız olması durumunda hatayı fırlat
                            
                            // Biraz daha bekle ve tekrar dene
                            await Task.Delay(1000);
                        }
                    }
                    
                    // Geçici kopyayı sıkıştırıp yedekle
                    using (var zipArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
                    {
                        zipArchive.CreateEntryFromFile(tempDbPath, Path.GetFileName(dbPath));
                    }
                    
                    Debug.WriteLine($"Veritabanı yedeklendi: {backupPath}");
                    return (true, backupPath, "Veritabanı başarıyla yedeklendi.");
                }
                finally
                {
                    // Geçici dosyayı temizle
                    if (File.Exists(tempDbPath))
                    {
                        try { File.Delete(tempDbPath); } catch { /* Geçici dosyayı silemezsek önemli değil */ }
                    }
                    
                    // Bağlantıyı yeniden açmaya çalış
                    try { await _context.Database.OpenConnectionAsync(); } catch { /* Bağlantıyı açamazsak önemli değil */ }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Yedekleme hatası: {ex.Message}");
                
                // Bağlantıyı yeniden açmaya çalış
                try { await _context.Database.OpenConnectionAsync(); } catch { /* Ignore */ }
                
                return (false, string.Empty, $"Yedekleme sırasında hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Yedeklenmiş veritabanını geri yükler
        /// </summary>
        public async Task<(bool Success, string Message)> RestoreDatabaseAsync(string backupPath)
        {
            if (string.IsNullOrEmpty(backupPath) || !File.Exists(backupPath))
            {
                return (false, "Yedek dosyası bulunamadı.");
            }
            
            try
            {
                // Veritabanı yolunu al
                string dbPath = AppDbContextFactory.GetDbPath();
                string tempDbPath = dbPath + ".temp";
                string extractPath = Path.Combine(Path.GetTempPath(), $"extract_db_{Guid.NewGuid()}.db");
                
                // Veritabanını kapat
                await _context.Database.CloseConnectionAsync();
                
                // SQLite'ın dosyayı serbest bırakması için bekle
                await Task.Delay(1000);
                
                // Mevcut veritabanını geçici olarak yedekle
                int retryCount = 0;
                bool backupSuccess = false;
                
                if (File.Exists(dbPath))
                {
                    while (!backupSuccess && retryCount < 5)
                    {
                        try
                        {
                            using (FileStream sourceStream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (FileStream tempStream = new FileStream(tempDbPath, FileMode.Create, FileAccess.Write))
                            {
                                await sourceStream.CopyToAsync(tempStream);
                                await tempStream.FlushAsync();
                            }
                            backupSuccess = true;
                        }
                        catch (IOException ex)
                        {
                            retryCount++;
                            Debug.WriteLine($"Geçici yedekleme hatası (deneme {retryCount}): {ex.Message}");
                            
                            if (retryCount >= 5)
                                return (false, $"Mevcut veritabanı kilitli ve erişilemiyor: {ex.Message}");
                            
                            await Task.Delay(1000);
                        }
                    }
                }
                
                try
                {
                    // Zip dosyasını aç ve içeriği çıkart
                    using (var zipArchive = ZipFile.OpenRead(backupPath))
                    {
                        var dbEntry = zipArchive.GetEntry(Path.GetFileName(dbPath));
                        if (dbEntry == null)
                        {
                            throw new FileNotFoundException("Yedek içinde veritabanı dosyası bulunamadı.");
                        }
                        
                        // Veritabanını geçici konuma çıkart
                        dbEntry.ExtractToFile(extractPath, true);
                    }
                    
                    // Geçici konumdan hedef konuma kopyala - dosya erişim sorunlarına karşı tekrar denemeli
                    retryCount = 0;
                    bool restoreSuccess = false;
                    
                    while (!restoreSuccess && retryCount < 5)
                    {
                        try
                        {
                            using (FileStream sourceStream = new FileStream(extractPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                // Varolan dosyayı silmeyi dene
                                if (File.Exists(dbPath))
                                {
                                    try { File.Delete(dbPath); } 
                                    catch (IOException)
                                    {
                                        retryCount++;
                                        if (retryCount >= 5)
                                            throw new IOException("Veritabanı dosyası kilitli ve silinemedi.");
                                        
                                        await Task.Delay(1000);
                                        continue;
                                    }
                                }
                                
                                using (FileStream targetStream = new FileStream(dbPath, FileMode.Create, FileAccess.Write))
                                {
                                    await sourceStream.CopyToAsync(targetStream);
                                    await targetStream.FlushAsync();
                                }
                                restoreSuccess = true;
                            }
                        }
                        catch (IOException ex)
                        {
                            retryCount++;
                            Debug.WriteLine($"Veritabanı geri yükleme hatası (deneme {retryCount}): {ex.Message}");
                            
                            if (retryCount >= 5)
                                throw new IOException($"Veritabanı dosyası hedefe kopyalanamadı: {ex.Message}", ex);
                            
                            await Task.Delay(1000);
                        }
                    }
                    
                    Debug.WriteLine($"Veritabanı geri yüklendi: {dbPath}");
                    
                    // Geçici dosyaları sil
                    try
                    {
                        if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
                        if (File.Exists(extractPath)) File.Delete(extractPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Geçici dosyalar silinirken hata: {ex.Message}");
                        // Bu önemli bir hata değil, işlemi durdurmaz
                    }
                    
                    return (true, "Veritabanı başarıyla geri yüklendi. Uygulama yeniden başlatılmalıdır.");
                }
                catch (Exception ex)
                {
                    // Hata oluşursa, geçici dosyayı geri al
                    if (backupSuccess && File.Exists(tempDbPath))
                    {
                        try
                        {
                            if (File.Exists(dbPath))
                            {
                                File.Delete(dbPath);
                            }
                            
                            using (FileStream sourceStream = new FileStream(tempDbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (FileStream targetStream = new FileStream(dbPath, FileMode.Create, FileAccess.Write))
                            {
                                await sourceStream.CopyToAsync(targetStream);
                                await targetStream.FlushAsync();
                            }
                        }
                        catch (Exception restoreEx)
                        {
                            Debug.WriteLine($"Geçici yedek geri alınırken ikinci hata: {restoreEx.Message}");
                        }
                    }
                    
                    Debug.WriteLine($"Geri yükleme hatası: {ex.Message}");
                    return (false, $"Geri yükleme sırasında hata oluştu: {ex.Message}");
                }
                finally
                {
                    // Geçici dosyaları temizle
                    try
                    {
                        if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
                        if (File.Exists(extractPath)) File.Delete(extractPath);
                    }
                    catch { /* Geçici dosyaları silemezsek önemli değil */ }
                    
                    // Bağlantıyı yeniden aç
                    try { await _context.Database.OpenConnectionAsync(); } catch { /* Ignore */ }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Geri yükleme hatası: {ex.Message}");
                
                // Bağlantıyı yeniden açmaya çalış
                try { await _context.Database.OpenConnectionAsync(); } catch { /* Ignore */ }
                
                return (false, $"Geri yükleme sırasında hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Mevcut yedekleri listeler
        /// </summary>
        public async Task<string[]> GetAvailableBackupsAsync()
        {
            await Task.CompletedTask; // Asenkron metod gereksinimleri için
            
            try
            {
                string backupFolder = GetBackupFolder();
                
                if (!Directory.Exists(backupFolder))
                {
                    return Array.Empty<string>();
                }
                
                var backupFiles = Directory.GetFiles(backupFolder, $"*{BACKUP_FILE_EXTENSION}")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToArray();
                
                return backupFiles;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Yedek listeleme hatası: {ex.Message}");
                return Array.Empty<string>();
            }
        }
        
        /// <summary>
        /// Otomatik yedekleme işlemi yapar
        /// </summary>
        public async Task<bool> AutoBackupAsync()
        {
            try
            {
                string backupFolder = GetBackupFolder();
                
                // Otomatik yedekleme klasörünü belirle
                string autoBackupFolder = Path.Combine(backupFolder, "Auto");
                
                if (!Directory.Exists(autoBackupFolder))
                {
                    Directory.CreateDirectory(autoBackupFolder);
                }
                
                // Otomatik yedek dosya adını oluştur
                string timestamp = DateTime.Now.ToString("yyyyMMdd");
                string backupPath = Path.Combine(autoBackupFolder, $"{BACKUP_FILE_PREFIX}Auto_{timestamp}{BACKUP_FILE_EXTENSION}");
                
                // Eğer bugün için zaten bir yedek varsa, onu güncelle
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                
                // Yedekleme yap
                var result = await BackupDatabaseAsync(backupPath);
                
                // Eski otomatik yedekleri temizle (son 7 günü sakla)
                CleanupOldAutoBackups(autoBackupFolder, 7);
                
                return result.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Otomatik yedekleme hatası: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Yedekleme klasörünü döndürür
        /// </summary>
        private string GetBackupFolder()
        {
            // Veri klasörü içinde Backups klasörü
            string dataFolder = AppDbContextFactory.GetDataFolderPath();
            return Path.Combine(dataFolder, BACKUP_FOLDER_NAME);
        }
        
        /// <summary>
        /// Eski otomatik yedekleri temizler
        /// </summary>
        private void CleanupOldAutoBackups(string autoBackupFolder, int daysToKeep)
        {
            try
            {
                if (!Directory.Exists(autoBackupFolder))
                {
                    return;
                }
                
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var oldBackups = Directory.GetFiles(autoBackupFolder, $"*{BACKUP_FILE_EXTENSION}")
                    .Where(f => File.GetLastWriteTime(f) < cutoffDate)
                    .ToList();
                
                foreach (var oldBackup in oldBackups)
                {
                    try
                    {
                        File.Delete(oldBackup);
                        Debug.WriteLine($"Eski yedek silindi: {oldBackup}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Eski yedek silinirken hata: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Eski yedekleri temizlerken hata: {ex.Message}");
            }
        }
    }
} 