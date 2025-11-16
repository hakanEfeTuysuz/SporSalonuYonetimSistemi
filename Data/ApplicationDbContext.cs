using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Bunu ekle
using Microsoft.EntityFrameworkCore; // Bunu ekle
using SporSalonuYonetimSistemi.Models; // Models klasörümüzü ekle

namespace SporSalonuYonetimSistemi.Data
{
	// IdentityDbContext kullanıyoruz çünkü üyelik sistemi (kullanıcılar, roller) istiyoruz.
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		// Oluşturduğumuz modelleri buraya DbSet olarak ekliyoruz.
		// Bunlar veritabanında tabloya dönüşecek.
		public DbSet<Hizmet> Hizmetler { get; set; }
		public DbSet<Antrenor> Antrenorler { get; set; }
		public DbSet<Randevu> Randevular { get; set; }
		public DbSet<AntrenorMusaitlik> AntrenorMusaitlikleri { get; set; }
	}
}