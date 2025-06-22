using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public class PersonelService : IPersonelService
    {
        private readonly AppDbContext _context;

        public PersonelService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Personnel>> GetAllAsync()
        {
            return await _context.Personnel.ToListAsync();
        }

        public async Task<Personnel?> GetByIdAsync(int id)
        {
            return await _context.Personnel.FindAsync(id);
        }

        public async Task<bool> AddAsync(Personnel personel)
        {
            try
            {
                await _context.Personnel.AddAsync(personel);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Personnel personel)
        {
            try
            {
                _context.Personnel.Update(personel);
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
                var personel = await GetByIdAsync(id);
                if (personel == null) return false;

                _context.Personnel.Remove(personel);
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