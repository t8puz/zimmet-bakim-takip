using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public class ZimmetService : IZimmetService
    {
        private readonly AppDbContext _context;

        public ZimmetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Assignment>> GetAllAsync()
        {
            return await _context.Assignments
                .Include(a => a.Device)
                .Include(a => a.Personnel)
                .ToListAsync();
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Device)
                .Include(a => a.Personnel)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> AddAsync(Assignment zimmet)
        {
            try
            {
                await _context.Assignments.AddAsync(zimmet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Assignment zimmet)
        {
            try
            {
                _context.Assignments.Update(zimmet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var zimmet = await GetByIdAsync(id);
                if (zimmet == null) return false;

                _context.Assignments.Remove(zimmet);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Assignment>> GetZimmetlerByPersonelIdAsync(int personelId)
        {
            return await _context.Assignments
                .Include(a => a.Device)
                .Include(a => a.Personnel)
                .Where(a => a.PersonnelId == personelId)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetZimmetlerByCihazIdAsync(int cihazId)
        {
            return await _context.Assignments
                .Include(a => a.Device)
                .Include(a => a.Personnel)
                .Where(a => a.DeviceId == cihazId)
                .ToListAsync();
        }
        
        public async Task<List<Assignment>> GetRecentAssignmentsAsync(int count)
        {
            return await _context.Assignments
                .Include(a => a.Device)
                .Include(a => a.Personnel)
                .OrderByDescending(a => a.AssignmentDate)
                .Take(count)
                .ToListAsync();
        }
    }
} 