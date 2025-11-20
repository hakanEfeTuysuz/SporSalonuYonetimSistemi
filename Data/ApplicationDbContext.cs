using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSistemi.Models;

namespace SporSalonuYonetimSistemi.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikleri { get; set; }
        // YENİ EKLENEN TABLO:
        public DbSet<AntrenorHizmet> AntrenorHizmetleri { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Çoka-Çok İlişki Ayarı
            builder.Entity<AntrenorHizmet>()
                .HasKey(ah => new { ah.AntrenorId, ah.HizmetId });

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Antrenor)
                .WithMany()
                .HasForeignKey(ah => ah.AntrenorId);

            builder.Entity<AntrenorHizmet>()
                .HasOne(ah => ah.Hizmet)
                .WithMany()
                .HasForeignKey(ah => ah.HizmetId);
        }
    }
}