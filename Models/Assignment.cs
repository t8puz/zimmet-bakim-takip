using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Cihaz seçimi zorunludur.")]
        public int DeviceId { get; set; }
        
        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
        
        [Required(ErrorMessage = "Personel seçimi zorunludur.")]
        public int PersonnelId { get; set; }
        
        [ForeignKey("PersonnelId")]
        public virtual Personnel? Personnel { get; set; }
        
        [StringLength(100, ErrorMessage = "Kullanıcı adı 100 karakterden uzun olamaz.")]
        public string UserName { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Departman 100 karakterden uzun olamaz.")]
        public string Department { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Zimmet tarihi zorunludur.")]
        public DateTime AssignmentDate { get; set; } = DateTime.Now;
        
        public DateTime? ReturnDate { get; set; }
        
        [StringLength(50, ErrorMessage = "Durum 50 karakterden uzun olamaz.")]
        public string Status { get; set; } = "Aktif";
        
        [StringLength(500, ErrorMessage = "Notlar 500 karakterden uzun olamaz.")]
        public string Notes { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Açıklama 500 karakterden uzun olamaz.")]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
} 