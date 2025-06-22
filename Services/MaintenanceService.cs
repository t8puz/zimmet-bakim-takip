using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;

        public MaintenanceService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()
        {
            try
            {
                return await _context.MaintenanceRecords
                    .Include(m => m.Device)
                    .OrderByDescending(m => m.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Bakım kayıtları alınırken hata oluştu", ex);
            }
        }

        public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int id)
        {
            try
            {
                return await _context.MaintenanceRecords
                    .Include(m => m.Device)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Bakım kaydı alınırken hata oluştu (ID: {id})", ex);
            }
        }

        public async Task<bool> AddMaintenanceRecordAsync(MaintenanceRecord record)
        {
            try
            {
                if (record == null)
                {
                    throw new ArgumentNullException(nameof(record));
                }

                await _context.MaintenanceRecords.AddAsync(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Bakım kaydı eklenirken hata oluştu", ex);
            }
        }

        public async Task<bool> UpdateMaintenanceRecordAsync(MaintenanceRecord record)
        {
            try
            {
                if (record == null)
                {
                    throw new ArgumentNullException(nameof(record));
                }

                var existingRecord = await _context.MaintenanceRecords.FindAsync(record.Id);
                if (existingRecord == null)
                {
                    throw new KeyNotFoundException($"ID: {record.Id} olan bakım kaydı bulunamadı.");
                }

                _context.Entry(existingRecord).CurrentValues.SetValues(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Bakım kaydı güncellenirken hata oluştu (ID: {record?.Id})", ex);
            }
        }

        public async Task<bool> DeleteMaintenanceRecordAsync(int id)
        {
            try
            {
                var record = await _context.MaintenanceRecords.FindAsync(id);
                if (record == null)
                {
                    throw new KeyNotFoundException($"ID: {id} olan bakım kaydı bulunamadı.");
                }

                _context.MaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Bakım kaydı silinirken hata oluştu (ID: {id})", ex);
            }
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceRecordsByDeviceIdAsync(int deviceId)
        {
            try
            {
                return await _context.MaintenanceRecords
                    .Where(m => m.DeviceId == deviceId)
                    .OrderByDescending(m => m.MaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Cihaz ID: {deviceId} için bakım kayıtları alınırken hata oluştu", ex);
            }
        }

        public async Task<int> GetOverdueMaintenanceCountAsync()
        {
            try
            {
                var today = DateTime.Today;
                return await _context.MaintenanceRecords
                    .Where(m => m.NextMaintenanceDate < today && m.Status != "Tamamlandı")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Gecikmiş bakım sayısı alınırken hata oluştu", ex);
            }
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int daysAhead)
        {
            try
            {
                var today = DateTime.Today;
                var endDate = today.AddDays(daysAhead);
                
                return await _context.MaintenanceRecords
                    .Include(m => m.Device)
                    .Where(m => m.NextMaintenanceDate >= today && m.NextMaintenanceDate <= endDate)
                    .OrderBy(m => m.NextMaintenanceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Yaklaşan bakımlar alınırken hata oluştu", ex);
            }
        }

        public async Task<decimal> GetTotalMaintenanceCostAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.MaintenanceRecords
                    .Where(m => m.MaintenanceDate >= startDate && m.MaintenanceDate <= endDate)
                    .SumAsync(m => m.Cost ?? 0);
            }
            catch (Exception ex)
            {
                throw new Exception("Toplam bakım maliyeti hesaplanırken hata oluştu", ex);
            }
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetRecentMaintenanceRecordsAsync(int count)
        {
            try
            {
                return await _context.MaintenanceRecords
                    .Include(m => m.Device)
                    .OrderByDescending(m => m.MaintenanceDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Son bakım kayıtları alınırken hata oluştu", ex);
            }
        }
        
        public async Task<IEnumerable<Maintenance>> GetAllAsync()
        {
            try
            {
                // Maintenances tablosu yerine MaintenanceRecords kullanarak dönüştürme yapalım
                var maintenanceRecords = await _context.MaintenanceRecords
                    .Include(m => m.Device)
                    .OrderByDescending(m => m.MaintenanceDate)
                    .ToListAsync();
                    
                // MaintenanceRecord nesnelerini Maintenance sınıfına dönüştür
                var maintenances = maintenanceRecords.Select(record => new Maintenance
                {
                    Id = record.Id,
                    DeviceId = record.DeviceId,
                    Device = record.Device,
                    MaintenanceDate = record.MaintenanceDate,
                    NextMaintenanceDate = record.NextMaintenanceDate,
                    MaintenanceType = record.MaintenanceType,
                    Status = record.Status,
                    Description = record.Description,
                    Cost = record.Cost,
                    Technician = record.TechnicianName,
                    Notes = record.Notes,
                    CreatedAt = record.CreatedAt,
                    UpdatedAt = record.UpdatedAt,
                    CreatedBy = record.CreatedBy,
                    UpdatedBy = record.UpdatedBy
                }).ToList();
                    
                return maintenances;
            }
            catch (Exception ex)
            {
                throw new Exception("Bakım kayıtları alınırken hata oluştu", ex);
            }
        }
        
        public async Task<bool> AddAsync(Maintenance maintenance)
        {
            try
            {
                if (maintenance == null)
                {
                    throw new ArgumentNullException(nameof(maintenance));
                }

                // Maintenance nesnesini MaintenanceRecord'a dönüştür
                var maintenanceRecord = new MaintenanceRecord
                {
                    DeviceId = maintenance.DeviceId,
                    MaintenanceDate = maintenance.MaintenanceDate,
                    NextMaintenanceDate = maintenance.NextMaintenanceDate,
                    MaintenanceType = maintenance.MaintenanceType,
                    Status = maintenance.Status,
                    Description = maintenance.Description,
                    Cost = maintenance.Cost,
                    TechnicianName = maintenance.Technician,
                    Notes = maintenance.Notes,
                    CreatedAt = maintenance.CreatedAt,
                    UpdatedAt = maintenance.UpdatedAt,
                    CreatedBy = maintenance.CreatedBy,
                    UpdatedBy = maintenance.UpdatedBy
                };

                await _context.MaintenanceRecords.AddAsync(maintenanceRecord);
                await _context.SaveChangesAsync();
                
                // Eklenen kaydın ID'sini orijinal nesneye geri ata
                maintenance.Id = maintenanceRecord.Id;
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Bakım kaydı eklenirken hata oluştu", ex);
            }
        }

        // İsim uyumsuzluğunu çözmek için eklenen metotlar
        public async Task<bool> AddMaintenanceAsync(Maintenance maintenance)
        {
            // AddAsync metodunu çağırıyoruz
            return await AddAsync(maintenance);
        }
        
        public async Task<bool> UpdateMaintenanceAsync(Maintenance maintenance)
        {
            try
            {
                if (maintenance == null)
                {
                    throw new ArgumentNullException(nameof(maintenance));
                }

                // Maintenance nesnesini MaintenanceRecord'a dönüştür
                var maintenanceRecord = new MaintenanceRecord
                {
                    Id = maintenance.Id,
                    DeviceId = maintenance.DeviceId,
                    MaintenanceDate = maintenance.MaintenanceDate,
                    NextMaintenanceDate = maintenance.NextMaintenanceDate,
                    MaintenanceType = maintenance.MaintenanceType,
                    Status = maintenance.Status,
                    Description = maintenance.Description,
                    Cost = maintenance.Cost,
                    TechnicianName = maintenance.Technician,
                    Notes = maintenance.Notes,
                    CreatedAt = maintenance.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = maintenance.CreatedBy,
                    UpdatedBy = maintenance.UpdatedBy,
                    IsCompleted = maintenance.Status == "Tamamlandı",
                    CompletedDate = maintenance.Status == "Tamamlandı" ? DateTime.Now : null
                };

                // UpdateMaintenanceRecordAsync metodunu çağırıyoruz
                return await UpdateMaintenanceRecordAsync(maintenanceRecord);
            }
            catch (Exception ex)
            {
                throw new Exception($"Bakım kaydı güncellenirken hata oluştu (ID: {maintenance?.Id})", ex);
            }
        }
    }
} 