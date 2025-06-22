using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Services
{
    public class CloudSyncService : ISyncService
    {
        private readonly ILogService _logService;
        private readonly HttpClient _httpClient;
        private readonly string _configPath;
        private bool _autoSyncEnabled;
        
        // API yapılandırma sabitleri
        private const string ApiBaseUrl = "https://api.zimmetbakimtakip.com/sync";
        // Gerçek bir uygulamada güvenli API anahtarı yönetimi kullanılmalıdır
        private const string ApiKey = "YOUR_API_KEY";

        public CloudSyncService(ILogService logService)
        {
            _logService = logService;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
            
            // Senkronizasyon yapılandırma dosyası
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
            
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _configPath = Path.Combine(appDataFolder, "sync_config.json");
            
            // Senkronizasyon yapılandırmasını yükle
            LoadSyncConfig();
        }
        
        private void LoadSyncConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<SyncConfig>(json);
                    _autoSyncEnabled = config?.AutoSyncEnabled ?? false;
                }
                else
                {
                    // Varsayılan yapılandırma
                    _autoSyncEnabled = false;
                    SaveSyncConfig();
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Senkronizasyon yapılandırması yüklenirken hata: {ex.Message}");
                _autoSyncEnabled = false;
            }
        }
        
        private void SaveSyncConfig()
        {
            try
            {
                var config = new SyncConfig
                {
                    AutoSyncEnabled = _autoSyncEnabled,
                    LastSync = DateTime.Now
                };
                
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Senkronizasyon yapılandırması kaydedilirken hata: {ex.Message}");
            }
        }
        
        public async Task<bool> SyncWithCloudAsync(string username)
        {
            try
            {
                _logService.LogInfo($"Bulut senkronizasyonu başlatılıyor: {username}");
                
                // Son senkronizasyon tarihini al
                DateTime lastSyncDate = GetLastSyncDate();
                
                // Sunucudaki son güncelleme tarihini kontrol et
                var checkResponse = await _httpClient.GetAsync($"{ApiBaseUrl}/check?username={username}&lastSync={lastSyncDate:o}");
                checkResponse.EnsureSuccessStatusCode();
                
                var checkResult = await checkResponse.Content.ReadFromJsonAsync<SyncCheckResult>();
                
                if (checkResult == null)
                {
                    _logService.LogError("Sunucu yanıtı geçersiz.");
                    return false;
                }
                
                // Sunucudaki veri daha yeni ise indir
                if (checkResult.ServerHasNewer)
                {
                    _logService.LogInfo("Sunucuda daha yeni veri bulundu, indiriliyor...");
                    return await DownloadFromCloudAsync(username);
                }
                
                // Yerel veri daha yeni ise yükle
                if (checkResult.ClientShouldUpload)
                {
                    _logService.LogInfo("Yerel veri daha yeni, yükleniyor...");
                    return await UploadToCloudAsync(username);
                }
                
                _logService.LogInfo("Veriler zaten senkronize.");
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Senkronizasyon hatası: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> UploadToCloudAsync(string username)
        {
            try
            {
                _logService.LogInfo($"Veri yükleniyor: {username}");
                
                // Veritabanı dosyasının yolunu al
                string dbFilePath = GetDatabaseFilePath();
                
                if (!File.Exists(dbFilePath))
                {
                    _logService.LogError("Veritabanı dosyası bulunamadı.");
                    return false;
                }
                
                // Veritabanı dosyasını oku
                byte[] dbContent = File.ReadAllBytes(dbFilePath);
                
                // Yükleme isteği için MultipartFormDataContent oluştur
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(dbContent), "database", "database.db");
                content.Add(new StringContent(username), "username");
                
                // API'ye yükle
                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/upload", content);
                response.EnsureSuccessStatusCode();
                
                // Başarılı yanıt kontrolü
                var result = await response.Content.ReadFromJsonAsync<SyncResult>();
                
                if (result == null || !result.Success)
                {
                    _logService.LogError("Yükleme başarısız: " + (result?.Message ?? "Bilinmeyen hata"));
                    return false;
                }
                
                // Son senkronizasyon tarihini güncelle
                UpdateLastSyncDate();
                
                _logService.LogInfo("Veri başarıyla yüklendi.");
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Veri yükleme hatası: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> DownloadFromCloudAsync(string username)
        {
            try
            {
                _logService.LogInfo($"Veri indiriliyor: {username}");
                
                // API'den veritabanını indir
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/download?username={username}");
                response.EnsureSuccessStatusCode();
                
                // İndirilen veritabanı verilerini al
                byte[] dbContent = await response.Content.ReadAsByteArrayAsync();
                
                if (dbContent == null || dbContent.Length == 0)
                {
                    _logService.LogError("İndirilen veri boş.");
                    return false;
                }
                
                // Yedek oluştur
                await BackupCurrentDatabaseAsync();
                
                // İndirilen veritabanını kaydet
                string dbFilePath = GetDatabaseFilePath();
                File.WriteAllBytes(dbFilePath, dbContent);
                
                // Son senkronizasyon tarihini güncelle
                UpdateLastSyncDate();
                
                _logService.LogInfo("Veri başarıyla indirildi.");
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError($"Veri indirme hatası: {ex.Message}");
                return false;
            }
        }
        
        private async Task BackupCurrentDatabaseAsync()
        {
            try
            {
                string dbFilePath = GetDatabaseFilePath();
                
                if (!File.Exists(dbFilePath))
                {
                    return;
                }
                
                string backupFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ZimmetBakimTakip", "Backups");
                
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFilePath = Path.Combine(backupFolder, $"database_before_sync_{timestamp}.db");
                
                // Dosyayı kopyala (async olarak)
                using (FileStream sourceStream = new FileStream(dbFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream destinationStream = new FileStream(backupFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
                
                _logService.LogInfo($"Senkronizasyon öncesi yedek oluşturuldu: {backupFilePath}");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Yedekleme hatası: {ex.Message}");
            }
        }
        
        private string GetDatabaseFilePath()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
            return Path.Combine(appDataFolder, "ZimmetBakim.db");
        }
        
        private DateTime GetLastSyncDate()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<SyncConfig>(json);
                    return config?.LastSync ?? DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Son senkronizasyon tarihi alınırken hata: {ex.Message}");
            }
            
            return DateTime.MinValue;
        }
        
        private void UpdateLastSyncDate()
        {
            try
            {
                var config = File.Exists(_configPath)
                    ? JsonSerializer.Deserialize<SyncConfig>(File.ReadAllText(_configPath))
                    : new SyncConfig();
                
                if (config != null)
                {
                    config.LastSync = DateTime.Now;
                    string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_configPath, json);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Son senkronizasyon tarihi güncellenirken hata: {ex.Message}");
            }
        }
        
        public bool GetAutoSyncStatus()
        {
            return _autoSyncEnabled;
        }
        
        public void SetAutoSyncStatus(bool status)
        {
            _autoSyncEnabled = status;
            SaveSyncConfig();
            _logService.LogInfo($"Otomatik senkronizasyon {(_autoSyncEnabled ? "etkinleştirildi" : "devre dışı bırakıldı")}");
        }
    }
    
    // Yapılandırma sınıfı
    public class SyncConfig
    {
        public bool AutoSyncEnabled { get; set; }
        public DateTime LastSync { get; set; }
    }
    
    // API yanıt modelleri
    public class SyncCheckResult
    {
        public bool ServerHasNewer { get; set; }
        public bool ClientShouldUpload { get; set; }
        public DateTime ServerLastUpdate { get; set; }
    }
    
    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
} 