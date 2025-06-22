using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Kullanıcı adı ve şifre ile giriş yapar
        /// </summary>
        Task<(bool Success, string Username)> LoginWithCredentialsAsync(string username, string password);
        
        /// <summary>
        /// Mevcut kullanıcının oturum durumunu kontrol eder
        /// </summary>
        bool IsLoggedIn();
        
        /// <summary>
        /// Mevcut kullanıcı bilgisini döndürür
        /// </summary>
        string GetCurrentUsername();
        
        /// <summary>
        /// Oturumu kapatır
        /// </summary>
        void Logout();
    }
} 