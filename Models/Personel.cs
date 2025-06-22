using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Zimmet_Bakim_Takip.Models
{
    public class Personel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Ad { get; set; } = string.Empty;
        
        [Required]
        public string Soyad { get; set; } = string.Empty;
        
        public string? Departman { get; set; }
        
        public string? Gorev { get; set; }
        
        public string? Email { get; set; }
        
        public string? Telefon { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        // İlişkiler
        public virtual ObservableCollection<Zimmet>? Zimmetler { get; set; }
        
        // Tam ad için yardımcı özellik
        public string TamAd => $"{Ad} {Soyad}";
    }
} 