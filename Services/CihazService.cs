using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public class CihazService : ICihazService
    {
        private readonly AppDbContext _context;

        public CihazService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            return await _context.Devices.ToListAsync();
        }

        public async Task<Device?> GetDeviceByIdAsync(int id)
        {
            return await _context.Devices.FindAsync(id);
        }

        public async Task<bool> AddDeviceAsync(Device device)
        {
            try
            {
                await _context.Devices.AddAsync(device);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDeviceAsync(Device device)
        {
            try
            {
                _context.Devices.Update(device);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDeviceAsync(int id)
        {
            try
            {
                var device = await GetDeviceByIdAsync(id);
                if (device == null) return false;

                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 