using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    public class Zimmet
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PersonelId { get; set; }
        
        [Required]
        public int CihazId { get; set; }
        
        [Required]
        public DateTime ZimmetTarihi { get; set; } = DateTime.Now;
        
        public DateTime? IadeTarihi { get; set; }
        
        public string? Aciklama { get; set; }
        
        // İlişkiler
        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }
        
        [ForeignKey("CihazId")]
        public virtual Cihaz? Cihaz { get; set; }
        
        // Zimmetin aktif olup olmadığını belirleyen property
        [NotMapped]
        public bool Aktif => IadeTarihi == null;
    }
} 