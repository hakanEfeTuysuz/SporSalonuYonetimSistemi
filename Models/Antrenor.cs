using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetimSistemi.Models
{
    public class Antrenor
    {
        [Key]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Antrenör adı zorunludur.")]
        [StringLength(50)]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Antrenör soyadı zorunludur.")]
        [StringLength(50)]
        public string Soyad { get; set; }

        [StringLength(200)]
        public string? UzmanlikAlanlari { get; set; }

        public string? FotografUrl { get; set; }

        // --- YENİ EKLENEN SAATLER ---
        [Display(Name = "Mesai Başlangıç")]
        [DataType(DataType.Time)]
        public TimeSpan CalismaBaslangic { get; set; } = new TimeSpan(9, 0, 0); // Varsayılan 09:00

        [Display(Name = "Mesai Bitiş")]
        [DataType(DataType.Time)]
        public TimeSpan CalismaBitis { get; set; } = new TimeSpan(18, 0, 0); // Varsayılan 18:00
    }
}