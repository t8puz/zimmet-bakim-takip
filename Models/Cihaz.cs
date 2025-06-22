using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Zimmet_Bakim_Takip.Models
{
    public class Cihaz
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Ad { get; set; } = string.Empty;
        
        [Required]
        public string Tur { get; set; } = string.Empty;
        
        public string? Marka { get; set; }
        
        public string? Model { get; set; }
        
        public string? SeriNo { get; set; }
        
        public DateTime? GarantiBaslangicTarihi { get; set; }
        
        public DateTime? GarantiBitisTarihi { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public string? Aciklama { get; set; }
        
        // İlişkiler
        public virtual ObservableCollection<Zimmet>? Zimmetler { get; set; }
        
        public virtual ObservableCollection<Bakim>? Bakimlar { get; set; }
    }
} 