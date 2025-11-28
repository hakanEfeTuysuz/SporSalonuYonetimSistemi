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

        // 3. Randevuyu Kaydetme (GÜNCELLENMİŞ VE DÜZELTİLMİŞ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur([Bind("RandevuTarihi,AntrenorId,HizmetId")] Randevu randevu)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            randevu.UyeId = userId;
            randevu.OnaylandiMi = false;

            // 1. GEÇMİŞ TARİH KONTROLÜ
            if (randevu.RandevuTarihi <= DateTime.Now)
            {
                ModelState.AddModelError("RandevuTarihi", "Lütfen bugünden ileri bir tarih ve saat seçiniz.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
            var secilenAntrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);

            if (secilenHizmet == null || secilenAntrenor == null)
            {
                ModelState.AddModelError("", "Hizmet veya Antrenör bulunamadı.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // Randevu Zamanlarını Hesapla
            var randevuBaslangic = randevu.RandevuTarihi;
            var randevuBitis = randevuBaslangic.AddMinutes(secilenHizmet.Sure);

            // --- 2. SALON SAATLERİ KONTROLÜ (KESİN ÇÖZÜM) ---
            // Randevunun alındığı günün sabah 09:00'ı ve akşam 22:00'ını tam tarih olarak belirliyoruz.
            DateTime salonAcilis = randevuBaslangic.Date.AddHours(9);  // Örn: 28.11.2025 09:00:00
            DateTime salonKapanis = randevuBaslangic.Date.AddHours(22); // Örn: 28.11.2025 22:00:00

            // Eğer başlangıç açılıştan önceyse VEYA bitiş kapanıştan sonraysa
            if (randevuBaslangic < salonAcilis || randevuBitis > salonKapanis)
            {
                ModelState.AddModelError("RandevuTarihi", $"Spor salonumuz 09:00 - 22:00 saatleri arasında açıktır. Seçtiğiniz hizmet {secilenHizmet.Sure} dakika sürdüğü için kapanış saatini geçmektedir.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // --- 3. ANTRENÖR MESAİ SAATLERİ KONTROLÜ ---
            // Sadece saat kısmını (TimeOfDay) karşılaştırıyoruz
            if (randevuBaslangic.TimeOfDay < secilenAntrenor.CalismaBaslangic ||
                randevuBitis.TimeOfDay > secilenAntrenor.CalismaBitis)
            {
                string bas = secilenAntrenor.CalismaBaslangic.ToString(@"hh\:mm");
                string bit = secilenAntrenor.CalismaBitis.ToString(@"hh\:mm");

                ModelState.AddModelError("", $"⛔ Bu antrenör sadece {bas} ile {bit} saatleri arasında çalışmaktadır.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // --- 4. ÇAKIŞMA (DOLULUK) KONTROLÜ ---
            var cakismaVarMi = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == randevu.AntrenorId && r.RandevuTarihi.Date == randevuBaslangic.Date)
                .AnyAsync(r =>
                    randevuBaslangic < r.RandevuTarihi.AddMinutes(r.Hizmet.Sure) &&
                    randevuBitis > r.RandevuTarihi
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
        [HttpGet]
        public async Task<IActionResult> GetUygunAntrenorler(int hizmetId)
        {
            var uygunIdler = await _context.AntrenorHizmetleri
                .Where(ah => ah.HizmetId == hizmetId)
                .Select(ah => ah.AntrenorId)
                .ToListAsync();

            var antrenorlerListesi = await _context.Antrenorler
                .Where(a => uygunIdler.Contains(a.AntrenorId))
                .ToListAsync();

            var sonuc = antrenorlerListesi.Select(a => new
            {
                value = a.AntrenorId,
                text = $"{a.Ad} {a.Soyad} 🕒 ({a.CalismaBaslangic:hh\\:mm} - {a.CalismaBitis:hh\\:mm})"
            });

            return Json(sonuc);
        }
    }
}