using System.Collections.Generic;
using System.Threading.Tasks;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IUserService
    {
        // Kimlik doğrulama
        Task<User?> AuthenticateAsync(string username, string password);
        
        // Kullanıcı yönetimi
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> AddUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user, string? password = null);
        Task<bool> DeleteUserAsync(int id);
        
        // Şifre yönetimi
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        
        // Rol kontrolü
        bool IsInRole(User user, string role);
    }
} 