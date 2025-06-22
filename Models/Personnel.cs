using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Zimmet_Bakim_Takip.Models
{
    public class Personnel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Department { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(50, ErrorMessage = "Pozisyon adı 50 karakterden uzun olamaz.")]
        public string Position { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [StringLength(150, ErrorMessage = "Adres 150 karakterden uzun olamaz.")]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        // İlişkiler
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        // Personel tam adını döndüren yardımcı özellik
        public string FullName => $"{FirstName} {LastName}";
    }
} 