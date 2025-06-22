using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public interface ICihazService
    {
        Task<List<Device>> GetAllDevicesAsync();
        Task<Device?> GetDeviceByIdAsync(int id);
        Task<bool> AddDeviceAsync(Device device);
        Task<bool> UpdateDeviceAsync(Device device);
        Task<bool> DeleteDeviceAsync(int id);
    }
} 