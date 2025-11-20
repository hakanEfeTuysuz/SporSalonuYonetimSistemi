using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSistemi.Data;
using SporSalonuYonetimSistemi.Models;

namespace SporSalonuYonetimSistemi.Controllers
{
    // DÜZELTME 1: Sadece [Authorize] yaptık. Böylece Üyeler de girebilir.
    [Authorize]
    public class AntrenorlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorlerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Herkes Listeyi Görebilir
        public async Task<IActionResult> Index()
        {
            return View(await _context.Antrenorler.ToListAsync());
        }

        // Herkes Detayları Görebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(m => m.AntrenorId == id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        // DÜZELTME 2: Ekleme işlemini SADECE ADMIN yapabilir
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Hizmetler = _context.Hizmetler.ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AntrenorId,Ad,Soyad,UzmanlikAlanlari,FotografUrl")] Antrenor antrenor, int[] selectedHizmetler)
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

        // DÜZELTME 3: Düzenleme işlemini SADECE ADMIN yapabilir
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
        public async Task<IActionResult> Edit(int id, [Bind("AntrenorId,Ad,Soyad,UzmanlikAlanlari,FotografUrl")] Antrenor antrenor, int[] selectedHizmetler)
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

        // DÜZELTME 4: Silme işlemini SADECE ADMIN yapabilir
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var antrenor = await _context.Antrenorler.FirstOrDefaultAsync(m => m.AntrenorId == id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null) _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorId == id);
        }
    }
}