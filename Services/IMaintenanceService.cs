using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IMaintenanceService
    {
        // Temel CRUD işlemleri
        Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync();
        Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int id);
        Task<bool> AddMaintenanceRecordAsync(MaintenanceRecord record);
        Task<bool> UpdateMaintenanceRecordAsync(MaintenanceRecord record);
        Task<bool> DeleteMaintenanceRecordAsync(int id);
        Task<IEnumerable<MaintenanceRecord>> GetMaintenanceRecordsByDeviceIdAsync(int deviceId);
        
        // İstatistiksel veya özetleyici metotlar
        Task<int> GetOverdueMaintenanceCountAsync();
        Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int daysAhead);
        Task<decimal> GetTotalMaintenanceCostAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MaintenanceRecord>> GetRecentMaintenanceRecordsAsync(int count);
        
        // Maintenance nesnesi için CRUD işlemleri
        Task<IEnumerable<Maintenance>> GetAllAsync();
        Task<bool> AddAsync(Maintenance maintenance);
        Task<bool> AddMaintenanceAsync(Maintenance maintenance);
        Task<bool> UpdateMaintenanceAsync(Maintenance maintenance);
    }
} 