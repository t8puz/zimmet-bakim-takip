using System;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Services
{
    /// <summary>
    /// Veri yedekleme ve geri yükleme işlemleri için arayüz
    /// </summary>
    public interface IDataBackupService
    {
        /// <summary>
        /// Veritabanını belirtilen dosya konumuna yedekler
        /// </summary>
        /// <param name="backupPath">Yedek dosyasının tam yolu (null ise otomatik oluşturulur)</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<(bool Success, string FilePath, string Message)> BackupDatabaseAsync(string backupPath = null);
        
        /// <summary>
        /// Yedeklenmiş veritabanını geri yükler
        /// </summary>
        /// <param name="backupPath">Yedek dosyasının tam yolu</param>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<(bool Success, string Message)> RestoreDatabaseAsync(string backupPath);
        
        /// <summary>
        /// Mevcut yedekleri listeler
        /// </summary>
        /// <returns>Yedek dosyalarının listesi</returns>
        Task<string[]> GetAvailableBackupsAsync();
        
        /// <summary>
        /// Otomatik yedekleme işlemi yapar (örn: uygulama kapanırken)
        /// </summary>
        /// <returns>İşlem başarılı ise true, değilse false</returns>
        Task<bool> AutoBackupAsync();
    }
} 