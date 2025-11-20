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

        // 3. Randevuyu Kaydetme (GÜNCELLENDİ: Tarih Kontrolü Eklendi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur([Bind("RandevuTarihi,AntrenorId,HizmetId")] Randevu randevu)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            randevu.UyeId = userId;
            randevu.OnaylandiMi = false;
            // --- MESAİ SAATİ KONTROLÜ ---
            // Sabah 09:00'dan önce veya Akşam 22:00'den sonraya izin verme
            if (randevu.RandevuTarihi.Hour < 9 || randevu.RandevuTarihi.Hour >= 22)
            {
                ModelState.AddModelError("RandevuTarihi", "Spor salonumuz 09:00 - 22:00 saatleri arasında hizmet vermektedir.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // --- YENİ EKLENEN TARİH KONTROLÜ ---
            // Eğer tarih seçilmediyse (0001) veya geçmiş bir tarihse hata ver
            if (randevu.RandevuTarihi <= DateTime.Now)
            {
                ModelState.AddModelError("RandevuTarihi", "Lütfen bugünden ileri bir tarih ve saat seçiniz.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }
            // -----------------------------------

            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
            if (secilenHizmet == null)
            {
                ModelState.AddModelError("", "Lütfen geçerli bir hizmet seçiniz.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

            // Çakışma Kontrolü
            var yeniBaslangic = randevu.RandevuTarihi;
            var yeniBitis = yeniBaslangic.AddMinutes(secilenHizmet.Sure);

            var cakismaVarMi = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == randevu.AntrenorId && r.RandevuTarihi.Date == yeniBaslangic.Date)
                .AnyAsync(r => yeniBaslangic < r.RandevuTarihi.AddMinutes(r.Hizmet.Sure) && yeniBitis > r.RandevuTarihi);

            if (cakismaVarMi)
            {
                ModelState.AddModelError("", $"Seçilen antrenör bu saat aralığında ({secilenHizmet.Sure} dk) müsait değil.");
                DropdownlariDoldur(randevu);
                return View(randevu);
            }

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

        // 5. YENİ EKLENEN: Admin Reddetme (Silme) Metodu
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu); // Randevuyü tamamen siler
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void DropdownlariDoldur(Randevu randevu)
        {
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "AntrenorId", "Ad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);
        }
        // --- YENİ EKLENEN: AJAX İÇİN FİLTRELEME METODU ---
        [HttpGet]
        public async Task<IActionResult> GetUygunAntrenorler(int hizmetId)
        {
            // 1. Bu hizmeti verebilen hocaların ID'lerini bul
            var uygunIdler = await _context.AntrenorHizmetleri
                .Where(ah => ah.HizmetId == hizmetId)
                .Select(ah => ah.AntrenorId)
                .ToListAsync();

            // 2. O hocaların isimlerini getir
            var antrenorler = await _context.Antrenorler
                .Where(a => uygunIdler.Contains(a.AntrenorId))
                .Select(a => new { value = a.AntrenorId, text = a.Ad + " " + a.Soyad })
                .ToListAsync();

            return Json(antrenorler);
        }

    }
}