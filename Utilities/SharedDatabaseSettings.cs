using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;

namespace Zimmet_Bakim_Takip.Utilities
{
    public class SharedDatabaseSettings
    {
        // Ayarlar dosyasının adı
        private const string SETTINGS_FILE_NAME = "shared_db_settings.json";
        
        // Ayarlar
        public string SharedFolderPath { get; set; } = string.Empty;
        public bool UseSharedDatabase { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        // Singleton örnek
        private static SharedDatabaseSettings _instance;
        
        // Ayarlar dosyasının tam yolu
        private static string SettingsFilePath 
        { 
            get 
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFolder = Path.Combine(appDataFolder, "ZimmetBakimTakip");
                
                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }
                
                return Path.Combine(settingsFolder, SETTINGS_FILE_NAME);
            } 
        }
        
        // Singleton örneği döndürme - thread safe
        public static SharedDatabaseSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }
        
        // Ayarları disk'ten yükleme
        private static SharedDatabaseSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<SharedDatabaseSettings>(json);
                    
                    // Null kontrolü
                    if (settings != null)
                    {
                        Debug.WriteLine("Paylaşımlı veritabanı ayarları yüklendi");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Paylaşımlı veritabanı ayarları yüklenirken hata: {ex.Message}");
            }
            
            // Varsayılan ayarları döndür
            Debug.WriteLine("Varsayılan paylaşımlı veritabanı ayarları oluşturuldu");
            return new SharedDatabaseSettings();
        }
        
        // Ayarları diske kaydetme
        public bool Save()
        {
            try
            {
                LastUpdated = DateTime.Now;
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
                Debug.WriteLine("Paylaşımlı veritabanı ayarları kaydedildi");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Paylaşımlı veritabanı ayarları kaydedilirken hata: {ex.Message}");
                return false;
            }
        }
    }
} 