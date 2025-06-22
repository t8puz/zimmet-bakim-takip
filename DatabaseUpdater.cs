using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Dapper;

namespace Zimmet_Bakim_Takip
{
    public static class DatabaseUpdater
    {
        /// <summary>
        /// Uygulama başlangıcında veritabanını günceller, eksik sütunları ekler
        /// </summary>
        public static void UpdateDatabase(string connectionString)
        {
            try
            {
                Debug.WriteLine("DatabaseUpdater: Veritabanı güncellemesi başlatılıyor...");
                
                // SQLite bağlantısı oluştur
                using (IDbConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    Debug.WriteLine("DatabaseUpdater: Veritabanına bağlantı kuruldu.");
                    
                    // Zimmet (Assignments) tablosu kontrolü
                    if (TableExists(connection, "Assignments"))
                    {
                        // Description sütunu ekle
                        if (!ColumnExists(connection, "Assignments", "Description"))
                        {
                            connection.Execute("ALTER TABLE Assignments ADD COLUMN Description TEXT");
                            Debug.WriteLine("DatabaseUpdater: Assignments tablosuna Description sütunu eklendi.");
                        }
                        
                        // IsActive sütunu ekle
                        if (!ColumnExists(connection, "Assignments", "IsActive"))
                        {
                            connection.Execute("ALTER TABLE Assignments ADD COLUMN IsActive INTEGER DEFAULT 1");
                            Debug.WriteLine("DatabaseUpdater: Assignments tablosuna IsActive sütunu eklendi.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("DatabaseUpdater: Assignments tablosu bulunamadı!");
                    }
                    
                    // Diğer güncellemeler buraya eklenebilir
                    
                    Debug.WriteLine("DatabaseUpdater: Veritabanı güncellemesi tamamlandı.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DatabaseUpdater HATA: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
            }
        }
        
        private static bool TableExists(IDbConnection connection, string tableName)
        {
            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
            var result = connection.QueryFirstOrDefault<string>(query, new { tableName });
            return result != null;
        }
        
        private static bool ColumnExists(IDbConnection connection, string tableName, string columnName)
        {
            try
            {
                var tableInfo = connection.Query($"PRAGMA table_info({tableName})").AsList();
                foreach (var column in tableInfo)
                {
                    if (column.name?.ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
} 