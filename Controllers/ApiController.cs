using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSistemi.Data;
using SporSalonuYonetimSistemi.Models;

namespace SporSalonuYonetimSistemi.Controllers
{
    [Route("api/[controller]")] // Bu API'ye ulaşma adresi: /api/Api
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GÖREV: Tüm Antrenörleri Listeleme (PDF Madde 4)
        // Adres: GET /api/Api/Antrenorler
        [HttpGet("Antrenorler")]
        public async Task<IActionResult> GetAntrenorler()
        {
            // LINQ Sorgusu: Veritabanından tüm veriyi çekmek yerine,
            // sadece ihtiyacımız olan kısımları (Ad, Soyad, Uzmanlık) seçiyoruz.
            // Ayrıca her antrenörün kaç randevusu olduğunu da saydırıyoruz.
            var antrenorler = await _context.Antrenorler
                .Select(a => new
                {
                    a.AntrenorId,
                    AdSoyad = a.Ad + " " + a.Soyad,
                    a.UzmanlikAlanlari,
                    // LINQ ile ilişkisel veri sayma (Örn: Bu hocanın kaç randevusu var?)
                    ToplamRandevu = _context.Randevular.Count(r => r.AntrenorId == a.AntrenorId)
                })
                .ToListAsync();

            return Ok(antrenorler); // Veriyi JSON formatında döndürür
        }

        // EKSTRA: Belirli bir tarihteki randevuları getir (PDF Örneği)
        // Adres: GET /api/Api/Randevular?tarih=2025-11-21
        [HttpGet("Randevular")]
        public async Task<IActionResult> GetRandevular(DateTime? tarih)
        {
            if (tarih == null) return BadRequest("Tarih girmelisiniz.");

            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.RandevuTarihi.Date == tarih.Value.Date) // LINQ Filtreleme
                .Select(r => new
                {
                    r.RandevuTarihi,
                    Antrenor = r.Antrenor.Ad + " " + r.Antrenor.Soyad,
                    Hizmet = r.Hizmet.Ad,
                    Durum = r.OnaylandiMi ? "Onaylı" : "Bekliyor"
                })
                .ToListAsync();

            return Ok(randevular);
        }
    }
}