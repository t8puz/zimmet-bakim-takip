using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    /// <summary>
    /// Sistemdeki tüm dosya eklerini temsil eden model
    /// </summary>
    public class Attachment
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string FilePath { get; set; } = string.Empty;
        
        [Required]
        public long FileSize { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RelatedEntityType { get; set; } = string.Empty; // "Device", "Maintenance", vb.
        
        [Required]
        public int RelatedEntityId { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string UploadedBy { get; set; } = string.Empty;
        
        // Dosyanın gerçek yolunu dönen yardımcı özellik (veritabanında saklanmaz)
        [NotMapped]
        public string FullPath => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments", FilePath);
        
        // Dosyanın türünü gösteren yardımcı özellik (PDF, resim vb.)
        [NotMapped]
        public string FileType
        {
            get
            {
                if (ContentType.StartsWith("image/"))
                    return "Resim";
                else if (ContentType == "application/pdf")
                    return "PDF";
                else if (ContentType.Contains("word"))
                    return "Word";
                else if (ContentType.Contains("excel") || ContentType.Contains("spreadsheet"))
                    return "Excel";
                else
                    return "Dosya";
            }
        }
    }
} 