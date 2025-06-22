using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IBakimService
    {
        Task<List<Bakim>> GetAllAsync();
        Task<Bakim?> GetByIdAsync(int id);
        Task<bool> AddAsync(Bakim bakim);
        Task<bool> UpdateAsync(Bakim bakim);
        Task<bool> DeleteAsync(int id);
        Task<bool> BakimTamamlaAsync(int bakimId, string? yapilanIslem);
        Task<List<Bakim>> GetBakimlarByCihazIdAsync(int cihazId);
        Task<List<Bakim>> GetBugunkuBakimlarAsync();
        Task<List<Bakim>> GetGecikmisBakimlarAsync();
        Task<List<Bakim>> GetGelecekBakimlarAsync();
        Task<List<Bakim>> GetTamamlananBakimlarAsync();
    }
} 