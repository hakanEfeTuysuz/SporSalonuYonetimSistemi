using System.ComponentModel.DataAnnotations; // Bu satırı ekle
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuYonetimSistemi.Models
{
	public class Hizmet
	{
		[Key] // Bu, 'HizmetId'nin anahtar (Primary Key) olduğunu belirtir.
		public int HizmetId { get; set; }

		[Required(ErrorMessage = "Hizmet adı zorunludur.")] // Bu alanın boş geçilemez olduğunu belirtir.
		[StringLength(100)] // Maksimum 100 karakter olabilir.
		public string Ad { get; set; }

		public int Sure { get; set; } // Hizmetin süresi (dakika)

		[Column(TypeName = "decimal(18, 2)")]
		public decimal Ucret { get; set; } // Hizmetin ücreti
	}
}
