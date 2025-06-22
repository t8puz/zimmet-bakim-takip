using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IZimmetService
    {
        Task<List<Assignment>> GetAllAsync();
        Task<Assignment?> GetByIdAsync(int id);
        Task<bool> AddAsync(Assignment zimmet);
        Task<bool> UpdateAsync(Assignment zimmet);
        Task<bool> DeleteAsync(int id);
        Task<List<Assignment>> GetZimmetlerByPersonelIdAsync(int personelId);
        Task<List<Assignment>> GetZimmetlerByCihazIdAsync(int cihazId);
        Task<List<Assignment>> GetRecentAssignmentsAsync(int count);
    }
} 