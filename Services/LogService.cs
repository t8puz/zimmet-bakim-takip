using System;
using System.IO;

namespace Zimmet_Bakim_Takip.Services
{
    public class LogService : ILogService
    {
        private readonly string _logFilePath;
        
        public LogService()
        {
            // Logları belgeler klasöründe sakla
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDataFolder = Path.Combine(documentsPath, "ZimmetBakimTakip");
            
            // Klasör yoksa oluştur
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _logFilePath = Path.Combine(appDataFolder, "application.log");
        }
        
        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public void LogError(string message)
        {
            Log("ERROR", message);
        }
        
        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        private void Log(string level, string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                
                // Dosyaya yaz (her seferinde ekleyerek)
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch
            {
                // Loglama hatası - bu durumda bir şey yapamayız
            }
        }
    }
} 