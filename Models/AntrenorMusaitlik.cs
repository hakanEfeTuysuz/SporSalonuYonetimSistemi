using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Bunu ekle

namespace SporSalonuYonetimSistemi.Models
{
	public class AntrenorMusaitlik
	{
		[Key]
		public int MusaitlikId { get; set; }

		// --- Foreign Key (İlişki) ---
		[Required]
		public int AntrenorId { get; set; } // Bu, Antrenor tablosuna bağlanacak

		[ForeignKey("AntrenorId")] // Yukarıdaki AntrenorId'nin bir Foreign Key olduğunu belirtir
		public virtual Antrenor? Antrenor { get; set; }
		// --- Bitiş ---

		[Required]
		public DayOfWeek Gun { get; set; } // Haftanın günü (Pazartesi, Salı vb.)

		[Required]
		public TimeSpan BaslangicSaati { get; set; } // Örn: 09:00

		[Required]
		public TimeSpan BitisSaati { get; set; } // Örn: 17:00
	}
}