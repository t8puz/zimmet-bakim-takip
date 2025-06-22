using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IGenericService<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
} 