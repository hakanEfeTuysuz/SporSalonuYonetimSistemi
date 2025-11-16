using System.ComponentModel.DataAnnotations; // Bu satırı ekle

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
		public string? UzmanlikAlanlari { get; set; } // '?' işareti bu alanın boş (null) olabileceğini belirtir.

		public string? FotografUrl { get; set; } // Antrenör fotoğrafının yolunu tutabiliriz (Boş olabilir)
	}
}