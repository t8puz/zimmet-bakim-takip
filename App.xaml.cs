using System;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Services;
using Zimmet_Bakim_Takip.ViewModels;
using Zimmet_Bakim_Takip.Utilities;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Linq;

namespace Zimmet_Bakim_Takip
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;
        
        // Servis sağlayıcıya dışarıdan erişim için
        public IServiceProvider Services => _serviceProvider!;
        
        public App()
        {
            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();
                
                // Beklenmeyen hata yakalayıcı ekleyelim
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                
                // Uygulama alanındaki yakalanmamış hataları yakala
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    try
                    {
                        var ex = e.ExceptionObject as Exception ?? new Exception("Bilinmeyen hata (UnhandledException)");
                        LogErrorToFile(ex, "AppDomain.UnhandledException");
                    }
                    catch { /* yut */ }
                };

                // Gözlemlenmemiş Task hatalarını yakala
                TaskScheduler.UnobservedTaskException += (s, e) =>
                {
                    try
                    {
                        LogErrorToFile(e.Exception, "TaskScheduler.UnobservedTaskException");
                        e.SetObserved();
                    }
                    catch { /* yut */ }
                };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Uygulama başlatılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
        
        // Beklenmeyen hataları yakalayalım
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                System.Windows.MessageBox.Show($"Beklenmeyen bir hata oluştu: {e.Exception.Message}\n\nUygulama çalışmaya devam edecek.", 
                               "Beklenmeyen Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Hatayı logla
                Debug.WriteLine($"Beklenmeyen hata: {e.Exception}");
                LogErrorToFile(e.Exception, "DispatcherUnhandledException");
            }
            catch
            {
                // En son çare: Hiçbir hata gösterme mekanizması çalışmıyorsa uygulamayı kapat
                Shutdown();
            }
        }
        
        /// <summary>
        /// Hataları uygulama klasöründeki Logs klasörüne yaz.
        /// </summary>
        private void LogErrorToFile(Exception ex, string source)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string logsDir = System.IO.Path.Combine(baseDir, "Logs");
                if (!System.IO.Directory.Exists(logsDir))
                {
                    System.IO.Directory.CreateDirectory(logsDir);
                }

                string fileName = $"error_{DateTime.Now:yyyyMMdd_HHmmss_fff}.log";
                string filePath = System.IO.Path.Combine(logsDir, fileName);

                var lines = new System.Collections.Generic.List<string>
                {
                    $"Kaynak: {source}",
                    $"Tarih: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}",
                    $"Mesaj: {ex.Message}",
                    $"Tür: {ex.GetType().FullName}",
                    "StackTrace:",
                    ex.StackTrace ?? "(yok)"
                };

                if (ex.InnerException != null)
                {
                    lines.Add("");
                    lines.Add("InnerException:");
                    lines.Add($"Mesaj: {ex.InnerException.Message}");
                    lines.Add($"Tür: {ex.InnerException.GetType().FullName}");
                    lines.Add("StackTrace:");
                    lines.Add(ex.InnerException.StackTrace ?? "(yok)");
                }

                System.IO.File.WriteAllLines(filePath, lines);
            }
            catch
            {
                // log yazılamıyorsa sessizce geç
            }
        }
        
        private ServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Önce DbContextOptions oluştur
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite($"Data Source={AppDbContextFactory.GetDbPath()};Mode=ReadWrite;");
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();

            // Veritabanı bağlantısı - hazır options kullan
            services.AddDbContext<AppDbContext>(options => 
            {
                options = optionsBuilder;
            }, ServiceLifetime.Scoped);

            // Loglama servisi - Singleton olarak kaydedildi
            services.AddSingleton<ILogService, LogService>();

            // Servisler - Scoped olarak kaydedildi
            services.AddScoped<ICihazService, CihazService>();
            services.AddScoped<IBakimService, BakimService>();
            services.AddScoped<IZimmetService, ZimmetService>();
            services.AddScoped<IPersonelService, PersonelService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            
            // Veri yedekleme servisi
            services.AddScoped<IDataBackupService, DataBackupService>();
            
            // Yeni servisler
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISyncService, CloudSyncService>();

            // ViewModels - Transient olarak kaydedildi
            services.AddTransient<MainViewModel>();
            /* Eksik ViewModels - geçici olarak yorum satırına alındı
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DevicesViewModel>();
            services.AddTransient<PersonnelViewModel>();
            services.AddTransient<AssignmentsViewModel>();
            services.AddTransient<MaintenanceViewModel>();
            // services.AddTransient<ReportsViewModel>(); // Raporlama kaldırıldı
            services.AddTransient<SettingsViewModel>();
            */

            // MainWindow kaydını kaldırdık, artık doğrudan oluşturuyoruz
            
            return services.BuildServiceProvider();
        }
        
        // Servis özellikleri - statik erişim için
        public static ICihazService CihazService => 
            ((App)Current)._serviceProvider!.GetRequiredService<ICihazService>();

        public static IBakimService BakimService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IBakimService>();

        public static IZimmetService ZimmetService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IZimmetService>();

        public static IPersonelService PersonelService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IPersonelService>();

        public static IMaintenanceService MaintenanceService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IMaintenanceService>();

        public static IUserService UserService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IUserService>();
        
        public static IDataBackupService DataBackupService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IDataBackupService>();
        
        public static IAuthService AuthService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IAuthService>();

        public static ISyncService SyncService => 
            ((App)Current)._serviceProvider!.GetRequiredService<ISyncService>();
        
        public static IAttachmentService AttachmentService => 
            ((App)Current)._serviceProvider!.GetRequiredService<IAttachmentService>();
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Ana pencereyi doğrudan göster (giriş ekranını atlayarak)
            var appMainWindow = new MainWindow();
            appMainWindow.Show();
            
            // Ana pencereyi başlangıçta göster
            this.MainWindow = appMainWindow;
            
            // Türkçe/Türkiye kültürünü ayarla
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("tr-TR");
            
            // Uygulama klasörleri oluştur
            InitializeAppFolders();

            // İlk çalıştırmada kısayol teklif et
            try
            {
                string dataFolderPath = AppDbContextFactory.GetDataFolderPath();
                Directory.CreateDirectory(dataFolderPath);
                string shortcutMarker = Path.Combine(dataFolderPath, "shortcut.created");

                if (!File.Exists(shortcutMarker))
                {
                    var result = MessageBox.Show(
                        "Masaüstü ve Başlat menüsüne kısayol oluşturulsun mu?",
                        "Kısayol Oluşturma",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                        if (!string.IsNullOrWhiteSpace(exePath) && File.Exists(exePath))
                        {
                            string appName = "Zimmet Bakım Takip";
                            bool desktopOk = ShortcutHelper.CreateDesktopShortcut(exePath, appName, exePath, "Zimmet Bakım Takip Sistemi");
                            bool startMenuOk = ShortcutHelper.CreateStartMenuShortcut(exePath, appName, exePath, "Zimmet Bakım Takip Sistemi");

                            // İşaret bırak
                            File.WriteAllText(shortcutMarker, $"{DateTime.Now:O}|desktop={desktopOk}|startMenu={startMenuOk}");

                            if (!desktopOk || !startMenuOk)
                            {
                                MessageBox.Show("Kısayol oluşturma sırasında bir sorun oluştu. Gerekirse manuel oluşturabilirsiniz.", 
                                    "Kısayol", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            // İşaret bırak ama uyar
                            File.WriteAllText(shortcutMarker, $"{DateTime.Now:O}|exe=not_found");
                            MessageBox.Show("Çalıştırılabilir dosya yolu tespit edilemedi. Kısayol oluşturulamadı.", 
                                "Kısayol", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        // Kullanıcı istemedi, bir daha sorma
                        File.WriteAllText(shortcutMarker, $"{DateTime.Now:O}|user_declined");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kısayol oluşturma denemesinde hata: {ex.Message}");
                // Sessizce devam et
            }
            
            // Veritabanı dosyasına erişimi kontrol et
            if (!AppDbContextFactory.CheckDatabaseFileAccess())
            {
                MessageBox.Show("Veritabanı dosyasına erişilemiyor. Başka bir uygulama bu dosyayı kullanıyor olabilir.\n\n" +
                              "Lütfen diğer açık uygulamaları kapatıp tekrar deneyin.", 
                              "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
            
            // DB Context ve servisleri oluştur
            try
            {
                // Services container
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                // Öncelikle veritabanı bağlamını oluştur (ama henüz kullanma)
                Debug.WriteLine("DbContext başarıyla kaydedildi.");
                
                // Veritabanı şemasını kontrol et ve güncelle
                try
                {
                    var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
                    bool dbExists = dbContext.Database.CanConnect();
                    Debug.WriteLine($"Veritabanı bağlantısı kontrol edildi: {(dbExists ? "Bağlantı başarılı" : "Bağlantı başarısız")}");
                    
                    if (!dbExists)
                    {
                        // Veritabanı yoksa oluştur
                        Debug.WriteLine("Veritabanı oluşturuluyor...");
                        dbContext.Database.EnsureCreated();
                        
                        // Başlangıç verilerini ekle
                        Task.Run(async () => {
                            try
                            {
                                await DbInitializer.InitializeAsync(dbContext);
                                Debug.WriteLine("Veritabanı başarıyla başlatıldı.");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"HATA: Veritabanı başlatılamadı: {ex.Message}");
                                if (ex.InnerException != null)
                                {
                                    Debug.WriteLine($"HATA: İç hata: {ex.InnerException.Message}");
                                }
                                
                                // Kullanıcıya bir uyarı göster
                                Dispatcher.Invoke(() => {
                                    MessageBox.Show($"Veritabanı başlatılamadı: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                                });
                            }
                        });
                    }
                    else
                    {
                        // Veritabanı mevcut, eksik sütunları güncelle
                        var connectionString = dbContext.Database.GetConnectionString();
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            // Veritabanı güncelleme işlemini çağır
                            DatabaseUpdater.UpdateDatabase(connectionString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"HATA: Veritabanı kontrolü sırasında hata: {ex.Message}");
                    MessageBox.Show($"Veritabanı bağlantısı kurulamadı: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
                
                // Servisleri başlat
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                var zimmetService = _serviceProvider.GetRequiredService<IZimmetService>();
                var personelService = _serviceProvider.GetRequiredService<IPersonelService>();
                var cihazService = _serviceProvider.GetRequiredService<ICihazService>();
                var bakimService = _serviceProvider.GetRequiredService<IBakimService>();
                var maintenanceService = _serviceProvider.GetRequiredService<IMaintenanceService>();
                var attachmentService = _serviceProvider.GetRequiredService<IAttachmentService>();
                
                Debug.WriteLine("Servisler başarıyla başlatıldı.");
                
                // Doğrudan ana pencereyi göster (giriş ekranını atla)
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HATA: Servis başlatılırken hata oluştu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"HATA: İç hata: {ex.InnerException.Message}");
                }
                
                // Kullanıcıya bir uyarı göster
                MessageBox.Show($"Uygulama başlatılamadı: {ex.Message}", "Başlangıç Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
        
        /// <summary>
        /// Uygulamanın ihtiyaç duyduğu klasörleri oluşturur ve yazma izinlerini kontrol eder
        /// </summary>
        private void InitializeAppFolders()
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                
                // Attachments klasörü
                string attachmentsFolder = Path.Combine(currentDirectory, "Attachments");
                if (!Directory.Exists(attachmentsFolder))
                {
                    Directory.CreateDirectory(attachmentsFolder);
                    Debug.WriteLine($"Attachments klasörü oluşturuldu: {attachmentsFolder}");
                }
                
                // Yazma izinlerini kontrol et
                try
                {
                    string testFile = Path.Combine(attachmentsFolder, "test_write.tmp");
                    File.WriteAllText(testFile, "Test");
                    File.Delete(testFile);
                    Debug.WriteLine("Attachments klasörü yazma izni kontrol edildi: OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UYARI: Attachments klasöründe yazma izni yok: {ex.Message}");
                    MessageBox.Show($"Dosya ekleri klasörü yazılabilir değil. Bazı özellikler çalışmayabilir.\n\nHata: {ex.Message}", 
                                 "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                // Logs klasörü
                string logsFolder = Path.Combine(currentDirectory, "Logs");
                if (!Directory.Exists(logsFolder))
                {
                    Directory.CreateDirectory(logsFolder);
                    Debug.WriteLine($"Logs klasörü oluşturuldu: {logsFolder}");
                }
                
                // Data klasörü
                string dataFolder = Path.Combine(currentDirectory, "Data");
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                    Debug.WriteLine($"Data klasörü oluşturuldu: {dataFolder}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HATA: Uygulama klasörleri oluşturulurken hata: {ex.Message}");
                MessageBox.Show($"Uygulama klasörleri oluşturulurken hata oluştu. Bazı özellikler çalışmayabilir.\n\nHata: {ex.Message}", 
                             "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Uygulama kapanırken otomatik yedekleme yap
                Debug.WriteLine("Uygulama kapanıyor, otomatik yedekleme yapılıyor...");
                
                // Yedekleme işlemini senkron olarak çalıştır (çünkü uygulama kapanıyor)
                var backupTask = DataBackupService.AutoBackupAsync();
                backupTask.Wait(); // Yedekleme tamamlanana kadar bekle
                
                var backupResult = backupTask.Result;
                if (backupResult)
                {
                    Debug.WriteLine("Otomatik yedekleme başarılı.");
                }
                else
                {
                    Debug.WriteLine("Otomatik yedekleme başarısız oldu.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Otomatik yedekleme sırasında hata: {ex.Message}");
            }
            finally
            {
                _serviceProvider?.Dispose();
                base.OnExit(e);
            }
        }
    }
}

