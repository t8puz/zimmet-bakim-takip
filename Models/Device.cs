using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    /// <summary>
    /// Cihaz veri modeli. 
    /// Bu sınıf, sistem tarafından takip edilen cihazları temsil eder.
    /// </summary>
    public enum DeviceStatus
    {
        Available,       // Kullanılabilir
        Assigned,        // Zimmetlenmiş
        InMaintenance,   // Bakımda
        Damaged,         // Hasarlı
        Retired          // Emekli/Hurdaya ayrılmış
    }

    public class Device
    {
        /// <summary>
        /// Cihazın benzersiz tanımlayıcısı
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Cihazın adı
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Cihazın seri numarası
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Cihazın model numarası
        /// </summary>
        [StringLength(50)]
        public string? Model { get; set; }

        /// <summary>
        /// Cihazın tipi
        /// </summary>
        [StringLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Cihazın markası
        /// </summary>
        [StringLength(100)]
        public string? Brand { get; set; }

        /// <summary>
        /// Cihazın kategorisi
        /// </summary>
        [StringLength(100, ErrorMessage = "Kategori 100 karakterden uzun olamaz.")]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Cihazın bağlı olduğu departman
        /// </summary>
        [StringLength(100, ErrorMessage = "Departman 100 karakterden uzun olamaz.")]
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Cihazın satın alınma tarihi
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// Cihazın satın alınma fiyatı
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Cihazın açıklaması
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Cihazın mevcut durumu
        /// </summary>
        [StringLength(50)]
        public string Status { get; set; } = "Available";

        /// <summary>
        /// Cihazın garanti bitiş tarihi
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Cihazın son bakım tarihi
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// Cihazın sonraki bakım tarihi
        /// </summary>
        public DateTime? NextMaintenanceDate { get; set; }

        /// <summary>
        /// Cihazın sisteme eklenme tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Cihaz kaydının son güncellenme tarihi
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Cihaz kaydının oluşturan kullanıcı
        /// </summary>
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Cihaz kaydının güncelleyen kullanıcı
        /// </summary>
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Cihazın konumu
        /// </summary>
        [StringLength(200)]
        public string? Location { get; set; }

        /// <summary>
        /// Cihazın aktiflik durumu
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Cihazın tedarikçisi
        /// </summary>
        [StringLength(100, ErrorMessage = "Tedarikçi adı 100 karakterden uzun olamaz.")]
        public string Supplier { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public virtual ICollection<Bakim> Bakimlar { get; set; } = new List<Bakim>();
        
        /// <summary>
        /// Cihaza ait dosya ekleri
        /// </summary>
        [NotMapped]
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        [NotMapped]
        public string Durum
        {
            get
            {
                if (Status == "InMaintenance")
                    return "Bakımda";
                else if (Status == "Assigned")
                    return "Zimmetli";
                else if (Status == "Available")
                    return "Müsait";
                else if (Status == "Damaged")
                    return "Hasarlı";
                else if (Status == "Retired")
                    return "Emekli";
                else
                    return Status;
            }
        }
    }
} 