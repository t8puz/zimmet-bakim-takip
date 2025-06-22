using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Services
{
    public interface ISyncService
    {
        /// <summary>
        /// Veritabanını bulut ile senkronize eder
        /// </summary>
        Task<bool> SyncWithCloudAsync(string username);
        
        /// <summary>
        /// Veritabanını buluta yükler
        /// </summary>
        Task<bool> UploadToCloudAsync(string username);
        
        /// <summary>
        /// Veritabanını buluttan indirir
        /// </summary>
        Task<bool> DownloadFromCloudAsync(string username);
        
        /// <summary>
        /// Otomatik senkronizasyon durumunu döndürür
        /// </summary>
        bool GetAutoSyncStatus();
        
        /// <summary>
        /// Otomatik senkronizasyon durumunu ayarlar
        /// </summary>
        void SetAutoSyncStatus(bool status);
    }
} 