using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public class BakimService : IBakimService
    {
        private readonly AppDbContext _context;

        public BakimService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Bakim>> GetAllAsync()
        {
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .ToListAsync();
        }

        public async Task<Bakim?> GetByIdAsync(int id)
        {
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> AddAsync(Bakim bakim)
        {
            try
            {
                await _context.Bakimlar.AddAsync(bakim);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Bakim bakim)
        {
            try
            {
                _context.Bakimlar.Update(bakim);
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
                var bakim = await GetByIdAsync(id);
                if (bakim == null) return false;

                _context.Bakimlar.Remove(bakim);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> BakimTamamlaAsync(int bakimId, string? yapilanIslem)
        {
            try
            {
                var bakim = await GetByIdAsync(bakimId);
                if (bakim == null) return false;

                bakim.Tamamlandi = true;
                bakim.YapilanIslem = yapilanIslem;
                bakim.GerceklesenTarih = DateTime.Now;
                bakim.GuncellenmeTarihi = DateTime.Now;
                bakim.GuncelleyenKullanici = Environment.UserName;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Bakim>> GetBakimlarByCihazIdAsync(int cihazId)
        {
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .Where(b => b.CihazId == cihazId)
                .ToListAsync();
        }

        public async Task<List<Bakim>> GetBugunkuBakimlarAsync()
        {
            var today = DateTime.Today;
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .Where(b => !b.Tamamlandi && b.PlanlananTarih.Date == today)
                .ToListAsync();
        }

        public async Task<List<Bakim>> GetGecikmisBakimlarAsync()
        {
            var today = DateTime.Today;
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .Where(b => !b.Tamamlandi && b.PlanlananTarih.Date < today)
                .ToListAsync();
        }

        public async Task<List<Bakim>> GetGelecekBakimlarAsync()
        {
            var today = DateTime.Today;
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .Where(b => !b.Tamamlandi && b.PlanlananTarih.Date > today)
                .ToListAsync();
        }

        public async Task<List<Bakim>> GetTamamlananBakimlarAsync()
        {
            return await _context.Bakimlar
                .Include(b => b.Cihaz)
                .Where(b => b.Tamamlandi)
                .ToListAsync();
        }
    }
} 