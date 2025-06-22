using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zimmet_Bakim_Takip.Database;
using Zimmet_Bakim_Takip.Models;
using BCrypt.Net;

namespace Zimmet_Bakim_Takip.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    System.Diagnostics.Debug.WriteLine("Kimlik doğrulama hatası: Kullanıcı adı veya şifre boş");
                    return null;
                }

                // Kullanıcıyı bulmaya çalış
                User? user = null;
                try 
                {
                    user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username);
                    
                    if (user == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Kimlik doğrulama hatası: '{username}' kullanıcısı bulunamadı");
                        return null;
                    }
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Veritabanı sorgu hatası: {dbEx.Message}");
                    throw new Exception("Kullanıcı bilgileri alınırken bir hata oluştu", dbEx);
                }

                // kullanıcı bulunamadı veya aktif değil
                if (!user.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine($"Kimlik doğrulama hatası: '{username}' kullanıcısı aktif değil");
                    return null;
                }

                // şifre kontrolü
                bool passwordVerified = false;
                try
                {
                    passwordVerified = VerifyPassword(password, user.PasswordHash);
                }
                catch (Exception hashEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Şifre doğrulama hatası: {hashEx.Message}");
                    throw new Exception("Şifre doğrulanırken bir hata oluştu", hashEx);
                }
                
                if (!passwordVerified)
                {
                    System.Diagnostics.Debug.WriteLine($"Kimlik doğrulama hatası: '{username}' için şifre doğrulanamadı");
                    return null;
                }

                // giriş başarılı, son giriş zamanını güncelle
                try
                {
                    user.LastLoginAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Kimlik doğrulama başarılı: '{username}' kullanıcısı giriş yaptı");
                }
                catch (Exception saveEx)
                {
                    // Son giriş zamanını güncelleyememe kritik bir hata değil
                    System.Diagnostics.Debug.WriteLine($"Son giriş zamanı güncellenirken hata: {saveEx.Message}");
                    // Kullanıcının giriş yapmasını engellemiyoruz, sadece logluyoruz
                }

                return user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Kullanıcı kimlik doğrulama hatası: {ex.Message}");
                // Hata ile ilgili ek bilgileri logla
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
                
                // Hatayı yukarı fırlat
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.Username == username);
        }

        public async Task<bool> AddUserAsync(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Şifre gerekli", nameof(password));

            if (await _context.Users.AnyAsync(x => x.Username == user.Username))
                throw new ArgumentException($"'{user.Username}' kullanıcı adı zaten kullanılıyor", nameof(user.Username));

            // şifreyi hash'le
            user.PasswordHash = HashPassword(password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUserAsync(User user, string? password = null)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return false;

            // kullanıcı adı değiştiyse çakışma kontrolü yap
            if (user.Username != existingUser.Username)
            {
                if (await _context.Users.AnyAsync(x => x.Username == user.Username))
                    throw new ArgumentException($"'{user.Username}' kullanıcı adı zaten kullanılıyor", nameof(user.Username));
            }

            // password varsa güncelle
            if (!string.IsNullOrWhiteSpace(password))
            {
                existingUser.PasswordHash = HashPassword(password);
            }

            // diğer özellikleri güncelle
            existingUser.Username = user.Username;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // mevcut şifreyi doğrula
            if (!VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            // yeni şifreyi hash'le ve kaydet
            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // şifreyi sıfırla (admin tarafından)
            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public bool IsInRole(User user, string role)
        {
            return user != null && user.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        // Yardımcı metotlar
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
} 