using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Zimmet_Bakim_Takip.Models;
using Microsoft.Win32;

namespace Zimmet_Bakim_Takip.Services
{
    public interface IAttachmentService
    {
        /// <summary>
        /// Belirtilen bağlı varlığa ait tüm ekleri getirir
        /// </summary>
        Task<List<Attachment>> GetAttachmentsAsync(string entityType, int entityId);
        
        /// <summary>
        /// ID'ye göre bir eki getirir
        /// </summary>
        Task<Attachment> GetByIdAsync(int id);
        
        /// <summary>
        /// Dosya seçme diyaloğunu açar ve seçilen dosyayı ilgili varlığa ekler
        /// </summary>
        Task<Attachment> AddFromFileDialogAsync(string entityType, int entityId, string description = "");
        
        /// <summary>
        /// Verilen dosya yolundan bir dosyayı ilgili varlığa ekler
        /// </summary>
        Task<Attachment> AddFromPathAsync(string entityType, int entityId, string filePath, string description = "");
        
        /// <summary>
        /// Bir ek kaydını veritabanından siler (dosya silinmez)
        /// </summary>
        Task<bool> DeleteAsync(int id);
        
        /// <summary>
        /// Bir ek kaydını ve ilişkili dosyayı tamamen siler
        /// </summary>
        Task<bool> DeleteWithFileAsync(int id);
        
        /// <summary>
        /// Bir eki açar (ilgili uygulamada görüntüler)
        /// </summary>
        bool OpenAttachment(Attachment attachment);
        
        /// <summary>
        /// Dosya seçme diyaloğunu açar
        /// </summary>
        OpenFileDialog CreateOpenFileDialog(bool multiSelect = false);
    }
} 