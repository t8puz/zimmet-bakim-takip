using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Utilities
{
    public static class DatabaseInitializer
    {
        public static readonly string DatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZimmetBakim.db");

        public static async Task InitializeAsync()
        {
            try
            {
                // Veritabanı dizinini oluştur
                var dbDirectory = Path.GetDirectoryName(DatabasePath);
                if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }

                // Veritabanı bağlantısını test et ve oluştur
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
                using var context = new AppDbContext(optionsBuilder.Options);
                await context.Database.EnsureCreatedAsync();

                // Örnek verileri ekle
                await SeedDataAsync(context);
            }
            catch (Exception ex)
            {
                throw new Exception($"Veritabanı başlatılırken hata oluştu: {ex.Message}", ex);
            }
        }

        private static async Task SeedDataAsync(AppDbContext context)
        {
            try
            {
                // Örnek cihazlar
                if (!await context.Devices.AnyAsync())
                {
                    var devices = new[]
                    {
                        new Device 
                        { 
                            Name = "HP LaserJet Pro", 
                            SerialNumber = "HP123456", 
                            Type = "Yazıcı", 
                            Status = "Müsait",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Device 
                        { 
                            Name = "Dell OptiPlex", 
                            SerialNumber = "DELL789012", 
                            Type = "Masaüstü", 
                            Status = "Müsait",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Device 
                        { 
                            Name = "Lenovo ThinkPad", 
                            SerialNumber = "LEN456789", 
                            Type = "Dizüstü", 
                            Status = "Müsait",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    };

                    await context.Devices.AddRangeAsync(devices);
                }

                // Örnek personel
                if (!await context.Personnel.AnyAsync())
                {
                    var personnel = new[]
                    {
                        new Personnel 
                        { 
                            FirstName = "Ahmet", 
                            LastName = "Yılmaz", 
                            Department = "Bilgi İşlem",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Personnel 
                        { 
                            FirstName = "Mehmet", 
                            LastName = "Kaya", 
                            Department = "Muhasebe",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        new Personnel 
                        { 
                            FirstName = "Ayşe", 
                            LastName = "Demir", 
                            Department = "İnsan Kaynakları",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    };

                    await context.Personnel.AddRangeAsync(personnel);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Örnek veriler eklenirken hata oluştu: {ex.Message}", ex);
            }
        }
    }
} 