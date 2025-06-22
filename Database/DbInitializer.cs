using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zimmet_Bakim_Takip.Models;
using BCrypt.Net;
using System.Diagnostics;
using System.Data;
using Dapper;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Zimmet_Bakim_Takip.Database
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            Debug.WriteLine("DbInitializer: Veritabanı başlatılıyor...");
            
            try
            {
                // Veritabanını tamamen sil ve yeniden oluştur (sıfırdan başla)
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                Debug.WriteLine("DbInitializer: Veritabanı yeniden oluşturuldu.");
                
                // Varsayılan kullanıcıları ekle
                await SeedUsersAsync(context);
                Debug.WriteLine("DbInitializer: Veritabanı başarıyla başlatıldı.");

                // SQLite bağlantısı açılır
                string connectionString = context.Database.GetConnectionString();
                using IDbConnection connection = new SQLiteConnection(connectionString);
                connection.Open();
                
                // Tabloları kontrol et
                var tableCheckResults = new Dictionary<string, bool>();
                var tables = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table'").ToList();
                
                foreach (var table in tables)
                {
                    tableCheckResults[table] = true;
                }

                // Zimmetler tablosu
                if (!tableCheckResults.ContainsKey("Assignments") || !tableCheckResults["Assignments"])
                {
                    connection.Execute(@"
                        CREATE TABLE Assignments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            DeviceId INTEGER NOT NULL,
                            PersonnelId INTEGER NOT NULL,
                            UserName TEXT NOT NULL,
                            Department TEXT,
                            AssignmentDate TEXT NOT NULL,
                            ReturnDate TEXT,
                            Status TEXT NOT NULL,
                            Notes TEXT,
                            CreatedAt TEXT NOT NULL,
                            UpdatedAt TEXT NOT NULL,
                            CreatedBy TEXT NOT NULL,
                            UpdatedBy TEXT NOT NULL,
                            FOREIGN KEY (DeviceId) REFERENCES Devices(Id),
                            FOREIGN KEY (PersonnelId) REFERENCES Personnel(Id)
                        )
                    ");
                    Debug.WriteLine("Assignments tablosu oluşturuldu.");
                }
                
                // Ekler (Attachments) tablosu
                if (!tableCheckResults.ContainsKey("Attachments") || !tableCheckResults["Attachments"])
                {
                    connection.Execute(@"
                        CREATE TABLE Attachments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FileName TEXT NOT NULL,
                            ContentType TEXT NOT NULL,
                            FilePath TEXT NOT NULL,
                            FileSize INTEGER NOT NULL,
                            RelatedEntityType TEXT NOT NULL,
                            RelatedEntityId INTEGER NOT NULL,
                            Description TEXT,
                            UploadDate TEXT NOT NULL,
                            UploadedBy TEXT NOT NULL
                        )
                    ");
                    Debug.WriteLine("Attachments tablosu oluşturuldu.");
                    
                    // İlişkili tablolar için indeks ekle
                    connection.Execute(@"
                        CREATE INDEX idx_attachments_relation 
                        ON Attachments (RelatedEntityType, RelatedEntityId)
                    ");
                    Debug.WriteLine("Attachments için indeks oluşturuldu.");
                }
                
                // Tablolara yeni sütunlar ekleme (varsa)
                UpdateExistingTables(connection);
                
                Debug.WriteLine("Veritabanı şema kontrolleri tamamlandı.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DbInitializer: HATA: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"DbInitializer: İç HATA: {ex.InnerException.Message}");
                }
                
                // Kritik bir hata olduğu için yukarı fırlat
                throw new Exception("Veritabanı başlatılamadı: " + ex.Message, ex);
            }
        }
        
        private static async Task SeedUsersAsync(AppDbContext context)
        {
            try
            {
                // Kullanıcı tablosunu kontrol et
                int userCount = await context.Users.CountAsync();
                Debug.WriteLine($"DbInitializer: Veritabanında {userCount} kullanıcı bulundu.");
                
                if (userCount == 0)
                {
                    Debug.WriteLine("DbInitializer: Varsayılan kullanıcılar ekleniyor...");
                    
                    // Admin Kullanıcı
                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FirstName = "Admin",
                        LastName = "Kullanıcı",
                        Email = "admin@firma.com",
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    
                    // Normal Kullanıcı
                    var normalUser = new User
                    {
                        Username = "kullanici",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("kullanici123"),
                        FirstName = "Normal",
                        LastName = "Kullanıcı",
                        Email = "kullanici@firma.com",
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    
                    await context.Users.AddRangeAsync(adminUser, normalUser);
                    await context.SaveChangesAsync();
                    Debug.WriteLine("DbInitializer: Varsayılan kullanıcılar başarıyla eklendi.");
                }
                else
                {
                    Debug.WriteLine("DbInitializer: Kullanıcılar zaten mevcut, atlıyorum.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DbInitializer: Kullanıcı Ekleme HATASI: {ex.Message}");
                throw new Exception("Kullanıcılar eklenirken hata oluştu: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Mevcut tablolara yeni sütunlar ekler
        /// </summary>
        private static void UpdateExistingTables(IDbConnection connection)
        {
            try
            {
                // Cihaz (Devices) tablosuna departman sütunu ekle
                if (!CheckColumnExists(connection, "Devices", "Department"))
                {
                    connection.Execute("ALTER TABLE Devices ADD COLUMN Department TEXT");
                    Debug.WriteLine("Devices tablosuna Department sütunu eklendi.");
                }
                
                // Bakım (Maintenance) tablosuna departman sütunu ekle
                if (!CheckColumnExists(connection, "Maintenance", "Department"))
                {
                    connection.Execute("ALTER TABLE Maintenance ADD COLUMN Department TEXT");
                    Debug.WriteLine("Maintenance tablosuna Department sütunu eklendi.");
                }
                
                // Zimmet (Assignments) tablosuna Description sütunu ekle
                if (!CheckColumnExists(connection, "Assignments", "Description"))
                {
                    connection.Execute("ALTER TABLE Assignments ADD COLUMN Description TEXT");
                    Debug.WriteLine("Assignments tablosuna Description sütunu eklendi.");
                }
                
                // Zimmet (Assignments) tablosuna IsActive sütunu ekle
                if (!CheckColumnExists(connection, "Assignments", "IsActive"))
                {
                    connection.Execute("ALTER TABLE Assignments ADD COLUMN IsActive INTEGER DEFAULT 1");
                    Debug.WriteLine("Assignments tablosuna IsActive sütunu eklendi.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Tablolar güncellenirken hata oluştu: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Bir tabloda belirli bir sütunun var olup olmadığını kontrol eder
        /// </summary>
        private static bool CheckColumnExists(IDbConnection connection, string tableName, string columnName)
        {
            try
            {
                var tableInfo = connection.Query($"PRAGMA table_info({tableName})").AsList();
                foreach (var column in tableInfo)
                {
                    if (column.name?.ToString()?.Equals(columnName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sütun kontrolü yapılırken hata: {ex.Message}");
                return false;
            }
        }
    }
} 