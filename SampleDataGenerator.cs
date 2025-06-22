using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Zimmet_Bakim_Takip.Models;
using System.Diagnostics;

namespace Zimmet_Bakim_Takip
{
    public static class SampleDataGenerator
    {
        /// <summary>
        /// Tüm örnek verileri oluşturur
        /// </summary>
        public static async Task GenerateAllSampleData()
        {
            try
            {
                // 15 adet cihaz oluştur
                await GenerateSampleDevices(15);

                // 15 adet personel oluştur
                await GenerateSamplePersonnel(15);

                // Cihaz ve personel listelerini al
                var devices = await App.CihazService.GetAllDevicesAsync();
                var personnel = await App.PersonelService.GetAllAsync();

                if (devices.Count > 0 && personnel.Count > 0)
                {
                    // 15 adet zimmet oluştur
                    await GenerateSampleAssignments(15, devices, personnel);
                    
                    // 15 adet bakım kaydı oluştur
                    await GenerateSampleMaintenanceRecords(15, devices);
                }
                else
                {
                    Debug.WriteLine("Zimmet ve bakım kaydı oluşturulamadı: Cihaz veya personel verisi yok");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Örnek veri oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Örnek cihazlar oluşturur
        /// </summary>
        private static async Task GenerateSampleDevices(int count)
        {
            string[] deviceNames = {
                "Dell Latitude 5420", "HP EliteBook 840", "Lenovo ThinkPad T14", "MacBook Pro 14", "Asus ZenBook Pro", 
                "Acer Aspire 5", "MSI GS66 Stealth", "Microsoft Surface Pro 8", "Samsung Galaxy Book", "Toshiba Tecra", 
                "Huawei MateBook", "LG Gram", "Razer Blade 15", "Gigabyte Aero", "Fujitsu Lifebook"
            };

            string[] serialNumbers = {
                "SN-87654321", "SN-12345678", "SN-ABCD1234", "SN-XYZ98765", "SN-QWE45678", 
                "SN-RTY12345", "SN-UIO98765", "SN-ASDF1234", "SN-HJKL5678", "SN-ZXCV9012", 
                "SN-BNMA3456", "SN-POIU7890", "SN-LKJH1234", "SN-MNBV5678", "SN-QAZX9012"
            };

            string[] models = {
                "T14s Gen 2", "EliteBook 850 G8", "XPS 13", "MacBook Pro M1", "ZenBook 14", 
                "Aspire 7", "GS66 Stealth 11UE", "Surface Pro X", "Galaxy Book Pro 360", "Tecra X40-E", 
                "MateBook X Pro", "Gram 17", "Blade 14", "Aero 15 OLED", "LifeBook U7410"
            };

            string[] departments = {
                "Muhasebe", "İnsan Kaynakları", "Bilgi Teknolojileri", "Pazarlama", "Satış", 
                "Müşteri Hizmetleri", "Üretim", "Lojistik", "Ar-Ge", "Hukuk", 
                "Eğitim", "Kalite Kontrol", "Güvenlik", "Yönetim", "Teknik Destek"
            };

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var device = new Device
                    {
                        Name = deviceNames[i % deviceNames.Length],
                        SerialNumber = serialNumbers[i % serialNumbers.Length] + "-" + i,
                        Model = models[i % models.Length],
                        PurchaseDate = DateTime.Now.AddMonths(-i),
                        Department = departments[i % departments.Length],
                        Status = i % 3 == 0 ? "Arızalı" : (i % 3 == 1 ? "Bakımda" : "Aktif"),
                        Notes = $"Cihaz {i+1} hakkında notlar.",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    };

                    await App.CihazService.AddDeviceAsync(device);
                    Debug.WriteLine($"Örnek cihaz {i+1} oluşturuldu: {device.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Örnek cihazlar oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Örnek personel kaydı oluşturur
        /// </summary>
        private static async Task GenerateSamplePersonnel(int count)
        {
            string[] firstNames = {
                "Ahmet", "Mehmet", "Ayşe", "Fatma", "Ali", 
                "Zeynep", "Mustafa", "Emine", "Hüseyin", "Hatice", 
                "İbrahim", "Elif", "Hasan", "Meryem", "Ömer"
            };

            string[] lastNames = {
                "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", 
                "Yıldız", "Aydın", "Özdemir", "Arslan", "Doğan", 
                "Kılıç", "Aslan", "Çetin", "Şimşek", "Koç"
            };

            string[] departments = {
                "Muhasebe", "İnsan Kaynakları", "Bilgi Teknolojileri", "Pazarlama", "Satış", 
                "Müşteri Hizmetleri", "Üretim", "Lojistik", "Ar-Ge", "Hukuk", 
                "Eğitim", "Kalite Kontrol", "Güvenlik", "Yönetim", "Teknik Destek"
            };

            string[] positions = {
                "Uzman", "Sorumlu", "Yönetici", "Asistan", "Şef", 
                "Direktör", "Danışman", "Teknisyen", "Mühendis", "Memur", 
                "Yönetmen", "Koordinatör", "Analist", "Supervisor", "Operatör"
            };

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var firstName = firstNames[i % firstNames.Length];
                    var lastName = lastNames[i % lastNames.Length];
                    
                    var personnel = new Personnel
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Department = departments[i % departments.Length],
                        Position = positions[i % positions.Length],
                        Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                        Phone = $"0555-{i:D3}-{(i*7) % 100:D2}-{(i*13) % 100:D2}",
                        Address = $"Örnek Mah. Test Sok. No:{i+1} D:{i+2}",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    };

                    await App.PersonelService.AddAsync(personnel);
                    Debug.WriteLine($"Örnek personel {i+1} oluşturuldu: {personnel.FirstName} {personnel.LastName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Örnek personel oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Örnek zimmet kayıtları oluşturur
        /// </summary>
        private static async Task GenerateSampleAssignments(int count, List<Device> devices, List<Personnel> personnel)
        {
            try
            {
                Random random = new Random();

                for (int i = 0; i < count; i++)
                {
                    int deviceIndex = random.Next(devices.Count);
                    int personnelIndex = random.Next(personnel.Count);

                    var device = devices[deviceIndex];
                    var person = personnel[personnelIndex];

                    var assignment = new Assignment
                    {
                        DeviceId = device.Id,
                        PersonnelId = person.Id,
                        Department = person.Department,
                        AssignmentDate = DateTime.Now.AddDays(-random.Next(1, 180)),
                        ReturnDate = random.Next(0, 2) == 0 ? DateTime.Now.AddDays(random.Next(30, 365)) : (DateTime?)null,
                        Status = random.Next(0, 3) == 0 ? "Tamamlandı" : (random.Next(0, 2) == 0 ? "İptal Edildi" : "Aktif"),
                        Notes = $"Zimmet {i+1} hakkında notlar.",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System",
                        UpdatedBy = "System",
                        IsActive = true,
                        Description = $"{device.Name} cihazı {person.FirstName} {person.LastName} personeline zimmetlendi."
                    };

                    await App.ZimmetService.AddAsync(assignment);
                    Debug.WriteLine($"Örnek zimmet {i+1} oluşturuldu: {device.Name} -> {person.FirstName} {person.LastName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Örnek zimmetler oluşturulurken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Örnek bakım kayıtları oluşturur
        /// </summary>
        private static async Task GenerateSampleMaintenanceRecords(int count, List<Device> devices)
        {
            string[] maintenanceTypes = {
                "Genel Bakım", "Periyodik Bakım", "Arıza Giderme", "Yazılım Güncelleme", "Donanım Değişimi",
                "Kalibrasyon", "Temizlik", "Parça Değişimi", "Performans İyileştirme", "Yükseltme",
                "Tam Revizyona", "Virüs Temizleme", "Disk Bakımı", "Ağ Yapılandırması", "Garanti Servisi"
            };

            string[] technicianNames = {
                "Ahmet Tekniker", "Mehmet Servis", "Ali Usta", "Veli Tamirci", "Ayşe Teknik",
                "Fatma Mühendis", "Hasan Bakım", "Hüseyin Destek", "İbrahim Servis", "Mustafa Teknik",
                "Zeynep Destek", "Elif Mühendis", "Meryem Tekniker", "Ömer Servis", "Hatice Bakım"
            };

            string[] statuses = {
                "Planlandı", "Devam Ediyor", "Tamamlandı", "İptal Edildi", "Ertelendi"
            };

            string[] departments = {
                "Muhasebe", "İnsan Kaynakları", "Bilgi Teknolojileri", "Pazarlama", "Satış", 
                "Müşteri Hizmetleri", "Üretim", "Lojistik", "Ar-Ge", "Hukuk", 
                "Eğitim", "Kalite Kontrol", "Güvenlik", "Yönetim", "Teknik Destek"
            };

            try
            {
                Random random = new Random();

                for (int i = 0; i < count; i++)
                {
                    int deviceIndex = random.Next(devices.Count);
                    var device = devices[deviceIndex];

                    var maintenanceDate = DateTime.Now.AddDays(-random.Next(1, 90));
                    var nextMaintenanceDate = maintenanceDate.AddDays(random.Next(30, 180));

                    // MaintenanceRecord nesnesi oluştur
                    var record = new MaintenanceRecord
                    {
                        DeviceId = device.Id,
                        MaintenanceDate = maintenanceDate,
                        NextMaintenanceDate = nextMaintenanceDate,
                        MaintenanceType = maintenanceTypes[i % maintenanceTypes.Length],
                        TechnicianName = technicianNames[i % technicianNames.Length],
                        Status = statuses[random.Next(statuses.Length)],
                        Cost = random.Next(1, 20) * 100.0m,
                        Description = $"{device.Name} için {maintenanceTypes[i % maintenanceTypes.Length]} yapıldı.",
                        Notes = $"Bakım {i+1} için notlar. İşlemler zamanında tamamlandı.",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    };

                    bool result = await App.MaintenanceService.AddMaintenanceRecordAsync(record);
                    
                    if (result)
                    {
                        // Aynı zamanda Maintenance tablosuna da kayıt ekle
                        var maintenance = new Maintenance
                        {
                            DeviceId = device.Id,
                            MaintenanceDate = maintenanceDate,
                            NextMaintenanceDate = nextMaintenanceDate,
                            MaintenanceType = maintenanceTypes[i % maintenanceTypes.Length],
                            Technician = technicianNames[i % technicianNames.Length],
                            Department = departments[i % departments.Length],
                            Status = statuses[random.Next(statuses.Length)],
                            Cost = random.Next(1, 20) * 100.0m,
                            Description = $"{device.Name} için {maintenanceTypes[i % maintenanceTypes.Length]} yapıldı.",
                            Notes = $"Bakım {i+1} için notlar. İşlemler zamanında tamamlandı.",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            CreatedBy = "System",
                            UpdatedBy = "System"
                        };

                        await App.MaintenanceService.AddAsync(maintenance);
                        Debug.WriteLine($"Örnek bakım {i+1} oluşturuldu: {device.Name} - {record.MaintenanceType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Örnek bakım kayıtları oluşturulurken hata: {ex.Message}");
                throw;
            }
        }
    }
} 