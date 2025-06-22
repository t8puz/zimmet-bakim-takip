using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Zimmet_Bakim_Takip.Utilities;
using System.Windows;

namespace Zimmet_Bakim_Takip.Database
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private static readonly string _dbFileName = "ZimmetBakim.db";
        private static string _dbPath;
        private static string _dataFolderName = "ZimmetBakimTakip";
        private static int _maxRetries = 3;
        private static TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

        static AppDbContextFactory()
        {
            try
            {
                // Paylaşımlı klasör ayarlarını kontrol et
                var settings = SharedDatabaseSettings.Instance;
                
                if (settings.UseSharedDatabase && !string.IsNullOrEmpty(settings.SharedFolderPath))
                {
                    try
                    {
                        // Paylaşımlı klasörü kullan
                        string sharedFolder = settings.SharedFolderPath;
                        
                        // Klasör mevcut mu kontrol et
                        if (!Directory.Exists(sharedFolder))
                        {
                            Debug.WriteLine($"Paylaşımlı klasör bulunamadı: {sharedFolder}");
                            throw new DirectoryNotFoundException($"Paylaşımlı klasör bulunamadı: {sharedFolder}");
                        }
                        
                        // Veritabanı dosya yolunu belirle
                        _dbPath = Path.Combine(sharedFolder, _dbFileName);
                        Debug.WriteLine($"Paylaşımlı veritabanı yolu: {_dbPath}");
                        
                        // Klasöre yazma yetkisi var mı kontrol et
                        try
                        {
                            string testFile = Path.Combine(sharedFolder, "test_write.tmp");
                            File.WriteAllText(testFile, "Test");
                            File.Delete(testFile);
                            Debug.WriteLine("Paylaşımlı klasöre yazma yetkisi mevcut");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Paylaşımlı klasöre yazma yetkisi yok: {ex.Message}");
                            throw new UnauthorizedAccessException("Paylaşımlı klasöre yazma yetkisi yok");
                        }
                        
                        return; // Paylaşımlı klasör başarıyla ayarlandı
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Paylaşımlı klasör kullanımında hata: {ex.Message}");
                        System.Windows.MessageBox.Show($"Paylaşımlı veritabanına erişim sağlanamadı: {ex.Message}\n\nYerel veritabanı kullanılacak.");
                        // Hata durumunda varsayılan klasöre düş
                    }
                }
                
                // Varsayılan: Verilerin saklanacağı klasörü belirle (Belgeler klasörü altında)
                string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dataFolder = Path.Combine(documentsFolder, _dataFolderName);
                
                Debug.WriteLine($"Veri klasörü: {dataFolder}");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                    Debug.WriteLine($"Veri klasörü oluşturuldu: {dataFolder}");
                }
                
                // Veritabanı dosya yolunu belirle
                _dbPath = Path.Combine(dataFolder, _dbFileName);
                Debug.WriteLine($"Veritabanı yolu: {_dbPath}");
                
                // Alternatif (eski) konum kontrolü
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string oldDbFolder = Path.Combine(appFolder, "Database");
                string oldDbPath = Path.Combine(oldDbFolder, _dbFileName);
                
                // Eğer eski konumda veritabanı varsa ve yeni konumda yoksa, taşı
                if (File.Exists(oldDbPath) && !File.Exists(_dbPath))
                {
                    Debug.WriteLine($"Eski veritabanı bulundu: {oldDbPath}");
                    File.Copy(oldDbPath, _dbPath, true);
                    Debug.WriteLine($"Veritabanı yeni konuma taşındı: {_dbPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Veritabanı yolu belirlenirken hata: {ex.Message}");
                
                // Hata durumunda alternatif konum kullan
                try
                {
                    string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                    string dbFolder = Path.Combine(appFolder, "Database");
                    
                    if (!Directory.Exists(dbFolder))
                    {
                        Directory.CreateDirectory(dbFolder);
                    }
                    
                    _dbPath = Path.Combine(dbFolder, _dbFileName);
                    Debug.WriteLine($"Alternatif veritabanı yolu kullanılıyor: {_dbPath}");
                }
                catch
                {
                    throw new Exception("Veritabanı yolu belirlenirken kritik hata oluştu.");
                }
            }
        }

        public AppDbContext CreateDbContext(string[] args)
        {
            try 
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={GetDbPath()}");

                return new AppDbContext(optionsBuilder.Options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DbContext oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        public static string GetDbPath()
        {
            if (string.IsNullOrEmpty(_dbPath))
            {
                throw new InvalidOperationException("Veritabanı yolu henüz belirlenmemiş.");
            }
            return _dbPath;
        }
        
        public static string GetDataFolderPath()
        {
            // Paylaşımlı klasör kontrolü
            var settings = SharedDatabaseSettings.Instance;
            if (settings.UseSharedDatabase && !string.IsNullOrEmpty(settings.SharedFolderPath) && Directory.Exists(settings.SharedFolderPath))
            {
                return settings.SharedFolderPath;
            }
            
            // Varsayılan konum
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsFolder, _dataFolderName);
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = $"Data Source={GetDbPath()};Mode=ReadWrite;";
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }, ServiceLifetime.Scoped);
        }
        
        /// <summary>
        /// Veritabanı dosyasına erişim sağlanabiliyor mu kontrol et
        /// </summary>
        public static bool CheckDatabaseFileAccess()
        {
            string dbPath = GetDbPath();
            
            if (!File.Exists(dbPath))
            {
                // Dosya henüz oluşturulmamış, sorun yok
                return true;
            }
            
            try
            {
                // Dosyayı sadece okuma modunda açarak testi gerçekleştir
                using (var stream = File.Open(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Dosya açılabildi, erişim var
                    return true;
                }
            }
            catch
            {
                // Dosya açılamadı, erişim sorunu var
                return false;
            }
        }
    }
} 