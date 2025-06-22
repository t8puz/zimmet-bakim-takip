using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogService _logService;
        private readonly string _configPath;
        
        private string? _currentUsername;
        private string? _currentEmail;
        private bool _isLoggedIn;
        
        public AuthService(ILogService logService)
        {
            _logService = logService;
            
            // Kullanıcı oturum bilgisi için config dosyası
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
            
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _configPath = Path.Combine(appDataFolder, "auth_config.json");
            
            // Oturum bilgisini yükle
            LoadSessionConfig();
        }
        
        private void LoadSessionConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<AuthConfig>(json);
                    
                    if (config != null)
                    {
                        _currentUsername = config.Username;
                        _currentEmail = config.Email;
                        _isLoggedIn = !string.IsNullOrEmpty(_currentUsername);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Oturum yapılandırması yüklenirken hata: {ex.Message}");
            }
        }
        
        private void SaveSessionConfig()
        {
            try
            {
                var config = new AuthConfig
                {
                    Username = _currentUsername,
                    Email = _currentEmail,
                    LastLogin = DateTime.Now
                };
                
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Oturum yapılandırması kaydedilirken hata: {ex.Message}");
            }
        }
        
        public async Task<(bool Success, string Username)> LoginWithCredentialsAsync(string username, string password)
        {
            try
            {
                _logService.LogInfo($"Kullanıcı giriş denemesi: {username}");
                
                // Bu metod UserService tarafından ele alınıyor, buraya bir şey eklemeye gerek yok
                // Burada sadece giriş başarılı olduğunda kullanıcı bilgilerini kaydedeceğiz
                
                _currentUsername = username;
                _currentEmail = username + "@localhost";
                _isLoggedIn = true;
                
                // Oturum bilgisini kaydet
                SaveSessionConfig();
                
                _logService.LogInfo($"Kullanıcı başarıyla giriş yaptı: {username}");
                return (true, username);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Giriş sırasında hata: {ex.Message}");
                return (false, string.Empty);
            }
        }
        
        public bool IsLoggedIn()
        {
            return _isLoggedIn;
        }
        
        public string GetCurrentUsername()
        {
            return _currentUsername ?? string.Empty;
        }
        
        public void Logout()
        {
            _currentUsername = string.Empty;
            _currentEmail = string.Empty;
            _isLoggedIn = false;
            
            // Oturum bilgisini güncelle
            SaveSessionConfig();
            
            _logService.LogInfo("Kullanıcı oturumu kapatıldı.");
        }
    }
    
    // Yapılandırma sınıfı
    public class AuthConfig
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime LastLogin { get; set; }
    }
} 