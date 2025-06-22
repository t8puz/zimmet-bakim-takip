using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zimmet_Bakim_Takip.Models
{
    public class Bakim
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CihazId { get; set; }
        
        [Required]
        public DateTime PlanlananTarih { get; set; }
        
        public DateTime? SonrakiTarih { get; set; }
        
        [Required]
        public string BakimTuru { get; set; } = string.Empty;
        
        [Required]
        public string Tur { get; set; } = string.Empty;
        
        public string? Teknisyen { get; set; }
        
        public string? Durum { get; set; } = "Planlandı";
        
        [StringLength(500)]
        public string? Aciklama { get; set; }
        
        [StringLength(500)]
        public string? Notlar { get; set; }
        
        public decimal? Maliyet { get; set; }
        
        public DateTime? GerceklesenTarih { get; set; }
        
        public string? YapilanIslem { get; set; }
        
        public bool Tamamlandi { get; set; } = false;
        
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
        
        public DateTime GuncellenmeTarihi { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string OlusturanKullanici { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string GuncelleyenKullanici { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("CihazId")]
        public virtual Device Cihaz { get; set; } = null!;
    }
} 