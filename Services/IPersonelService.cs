using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IPersonelService
    {
        Task<List<Personnel>> GetAllAsync();
        Task<Personnel?> GetByIdAsync(int id);
        Task<bool> AddAsync(Personnel personel);
        Task<bool> UpdateAsync(Personnel personel);
        Task<bool> DeleteAsync(int id);
    }
} 