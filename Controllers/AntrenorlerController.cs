using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSistemi.Data;
using SporSalonuYonetimSistemi.Models;

namespace SporSalonuYonetimSistemi.Controllers
{
    [Authorize] // Üyeler listeyi görebilsin
    public class AntrenorlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorlerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Antrenorler.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(m => m.AntrenorId == id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        // --- CREATE ---
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Hizmetler = _context.Hizmetler.ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GÜNCELLENDİ: Bind içine CalismaBaslangic ve CalismaBitis EKLENDİ
        public async Task<IActionResult> Create([Bind("AntrenorId,Ad,Soyad,UzmanlikAlanlari,FotografUrl,CalismaBaslangic,CalismaBitis")] Antrenor antrenor, int[] selectedHizmetler)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();

                if (selectedHizmetler != null)
                {
                    foreach (var hizmetId in selectedHizmetler)
                    {
                        _context.AntrenorHizmetleri.Add(new AntrenorHizmet { AntrenorId = antrenor.AntrenorId, HizmetId = hizmetId });
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Hizmetler = _context.Hizmetler.ToList();
            return View(antrenor);
        }

        // --- EDIT ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewBag.Hizmetler = _context.Hizmetler.ToList();
            ViewBag.SeciliHizmetler = await _context.AntrenorHizmetleri
                .Where(ah => ah.AntrenorId == id).Select(ah => ah.HizmetId).ToListAsync();

            return View(antrenor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GÜNCELLENDİ: Bind içine CalismaBaslangic ve CalismaBitis EKLENDİ
        public async Task<IActionResult> Edit(int id, [Bind("AntrenorId,Ad,Soyad,UzmanlikAlanlari,FotografUrl,CalismaBaslangic,CalismaBitis")] Antrenor antrenor, int[] selectedHizmetler)
        {
            if (id != antrenor.AntrenorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();

                    var eskiler = _context.AntrenorHizmetleri.Where(ah => ah.AntrenorId == id);
                    _context.AntrenorHizmetleri.RemoveRange(eskiler);
                    await _context.SaveChangesAsync();

                    if (selectedHizmetler != null)
                    {
                        foreach (var hizmetId in selectedHizmetler)
                        {
                            _context.AntrenorHizmetleri.Add(new AntrenorHizmet { AntrenorId = id, HizmetId = hizmetId });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.AntrenorId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Hizmetler = _context.Hizmetler.ToList();
            ViewBag.SeciliHizmetler = await _context.AntrenorHizmetleri
                .Where(ah => ah.AntrenorId == id).Select(ah => ah.HizmetId).ToListAsync();
            return View(antrenor);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(m => m.AntrenorId == id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        // POST: Antrenorler/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null)
            {
                return NotFound();
            }

            // --- GÜVENLİK KONTROLÜ BAŞLANGIÇ ---
            // Bu hocaya ait herhangi bir randevu var mı?
            bool randevusuVarMi = await _context.Randevular.AnyAsync(r => r.AntrenorId == id);

            if (randevusuVarMi)
            {
                // Hata mesajı oluştur ve sayfayı tekrar göster (SİLME!)
                ViewBag.ErrorMessage = "⛔ BU ANTRENÖR SİLİNEMEZ! Çünkü sisteme kayıtlı randevuları bulunmaktadır. Önce randevuları silmelisiniz.";
                return View("Delete", antrenor);
            }
            // --- GÜVENLİK KONTROLÜ BİTİŞ ---

            _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorId == id);
        }
    }
}