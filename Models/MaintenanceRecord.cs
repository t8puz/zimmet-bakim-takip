using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [ForeignKey("DeviceId")]
        public Device Device { get; set; } = null!;

        [Required]
        public DateTime MaintenanceDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? TechnicianName { get; set; }

        public decimal? Cost { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        [Required]
        [StringLength(50)]
        public string MaintenanceType { get; set; } = string.Empty;

        [Required]
        public bool IsCompleted { get; set; }

        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
    }
} 