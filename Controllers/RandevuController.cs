using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSistemi.Data;
using SporSalonuYonetimSistemi.Models;
using System.Linq;
using System.Security.Claims;

namespace SporSalonuYonetimSistemi.Controllers
{
    [Authorize(Roles = "Uye,Admin")]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Listeleme
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userIsAdmin = User.IsInRole("Admin");

            var randevularQuery = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .AsQueryable();

            if (!userIsAdmin)
            {
                randevularQuery = randevularQuery.Where(r => r.UyeId == userId);
            }

            var randevular = await randevularQuery
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();

            return View(randevular);
        }

        // 2. Randevu Alma Sayfası
        public IActionResult Olustur()
        {
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "AntrenorId", "Ad");
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad");
            return View();
        }

        // 3. Randevuyu Kaydetme (TAM GÜVENLİK KONTROLLÜ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur([Bind("RandevuTarihi,AntrenorId,HizmetId")] Randevu randevu)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            randevu.UyeId = userId;
            randevu.OnaylandiMi = false;

            // KONTROL 1: Tarih Geçmişte mi?
            if (randevu.RandevuTarihi <= DateTime.Now)
            {
                ModelState.AddModelError("RandevuTarihi", "Lütfen bugünden ileri bir tarih ve saat seçiniz.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // Bilgileri Çek
            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
            var secilenAntrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);

            if (secilenHizmet == null || secilenAntrenor == null)
            {
                ModelState.AddModelError("", "Hizmet veya Antrenör bulunamadı.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // Başlangıç ve Bitiş Hesapla
            var yeniBaslangic = randevu.RandevuTarihi;
            var yeniBitis = yeniBaslangic.AddMinutes(secilenHizmet.Sure);

            // KONTROL 2: SALON SAATLERİ (09:00 - 22:00)
            if (yeniBaslangic.Hour < 9 || yeniBitis.Hour >= 22 || (yeniBitis.Hour == 22 && yeniBitis.Minute > 0))
            {
                ModelState.AddModelError("RandevuTarihi", "Spor salonumuz sadece 09:00 - 22:00 saatleri arasında açıktır.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // KONTROL 3: ANTRENÖR MESAİ SAATLERİ
            TimeSpan baslangicSaati = yeniBaslangic.TimeOfDay;
            TimeSpan bitisSaati = yeniBitis.TimeOfDay;

            if (baslangicSaati < secilenAntrenor.CalismaBaslangic ||
                bitisSaati > secilenAntrenor.CalismaBitis)
            {
                string bas = secilenAntrenor.CalismaBaslangic.ToString(@"hh\:mm");
                string bit = secilenAntrenor.CalismaBitis.ToString(@"hh\:mm");
                ModelState.AddModelError("", $"⛔ Bu antrenör sadece {bas} ile {bit} saatleri arasında çalışmaktadır.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // KONTROL 4: ÇAKIŞMA (DOLULUK) KONTROLÜ
            var cakismaVarMi = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == randevu.AntrenorId && r.RandevuTarihi.Date == yeniBaslangic.Date)
                .AnyAsync(r =>
                    yeniBaslangic < r.RandevuTarihi.AddMinutes(r.Hizmet.Sure) &&
                    yeniBitis > r.RandevuTarihi
                );

            if (cakismaVarMi)
            {
                ModelState.AddModelError("", "⚠️ Seçtiğiniz antrenörün bu saatte başka bir randevusu var (Dolu).");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // KAYDET
            _context.Add(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // 4. Admin Onaylama
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.OnaylandiMi = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. Admin Reddetme
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void DropdownlariDoldur(Randevu randevu)
        {
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "AntrenorId", "Ad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);
        }

        // AJAX Filtreleme
        // --- GÜNCELLENEN: AJAX İÇİN FİLTRELEME METODU (SAATLER DAHİL) ---
        [HttpGet]
        public async Task<IActionResult> GetUygunAntrenorler(int hizmetId)
        {
            // 1. Bu hizmeti verebilen hocaların ID'lerini bul
            var uygunIdler = await _context.AntrenorHizmetleri
                .Where(ah => ah.HizmetId == hizmetId)
                .Select(ah => ah.AntrenorId)
                .ToListAsync();

            // 2. O hocaları veritabanından çek (Önce veriyi hafızaya alıyoruz)
            var antrenorlerListesi = await _context.Antrenorler
                .Where(a => uygunIdler.Contains(a.AntrenorId))
                .ToListAsync();

            // 3. İsimleri ve SAATLERİ formatlayıp gönder
            // Örnek Çıktı: "Ahmet Yılmaz 🕒 (09:00 - 17:00)"
            var sonuc = antrenorlerListesi.Select(a => new
            {
                value = a.AntrenorId,
                text = $"{a.Ad} {a.Soyad} 🕒 ({a.CalismaBaslangic:hh\\:mm} - {a.CalismaBitis:hh\\:mm})"
            });

            return Json(sonuc);
        }
    }
}