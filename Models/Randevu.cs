using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Bunu ekle (IdentityUser için)

namespace SporSalonuYonetimSistemi.Models
{
    [Authorize(Roles = "Uye,Admin")]
    public class Randevu
	{
		[Key]
		public int RandevuId { get; set; }

		[Required]
		public DateTime RandevuTarihi { get; set; } // Sadece tarih değil, saat de içerir

		public bool OnaylandiMi { get; set; } = false; // Randevu ilk oluşturulduğunda onaysız olsun

		// --- Foreign Key (İlişki): Üye ---
		[Required]
		public string UyeId { get; set; } // IdentityUser'ın Id'si string'dir

		[ForeignKey("UyeId")]
		public virtual IdentityUser? Uye { get; set; } // ASP.NET Core Identity'nin User tablosuna bağlanacak
													   // --- Bitiş ---

		// --- Foreign Key (İlişki): Antrenör ---
		[Required]
		public int AntrenorId { get; set; }

		[ForeignKey("AntrenorId")]
		public virtual Antrenor? Antrenor { get; set; }
		// --- Bitiş ---

		// --- Foreign Key (İlişki): Hizmet ---
		[Required]
		public int HizmetId { get; set; }

		[ForeignKey("HizmetId")]
		public virtual Hizmet? Hizmet { get; set; }
		// --- Bitiş ---
	}
}