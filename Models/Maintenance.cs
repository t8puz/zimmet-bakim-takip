using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Cihaz seçimi zorunludur.")]
        public int DeviceId { get; set; }
        
        [Required(ErrorMessage = "Bakım tarihi zorunludur.")]
        public DateTime MaintenanceDate { get; set; } = DateTime.Now;
        
        public DateTime? NextMaintenanceDate { get; set; }
        
        [Required(ErrorMessage = "Bakım türü zorunludur.")]
        [StringLength(100, ErrorMessage = "Bakım türü 100 karakterden uzun olamaz.")]
        public string MaintenanceType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Bakım durumu zorunludur.")]
        [StringLength(50, ErrorMessage = "Bakım durumu 50 karakterden uzun olamaz.")]
        public string Status { get; set; } = "Tamamlandı";
        
        [StringLength(500, ErrorMessage = "Açıklama 500 karakterden uzun olamaz.")]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99, ErrorMessage = "Bakım maliyeti negatif olamaz.")]
        public decimal? Cost { get; set; }
        
        [StringLength(100, ErrorMessage = "Teknisyen adı 100 karakterden uzun olamaz.")]
        public string Technician { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Bakım yapan firma 100 karakterden uzun olamaz.")]
        public string Company { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Departman 100 karakterden uzun olamaz.")]
        public string Department { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Notlar 1000 karakterden uzun olamaz.")]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        
        // İlişkiler
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
        
        /// <summary>
        /// Bakıma ait dosya ekleri
        /// </summary>
        [NotMapped]
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
} 