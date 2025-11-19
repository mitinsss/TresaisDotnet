using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PraktiskaisDarbs3.Data;
using PraktiskaisDarbs3.Models;

namespace PraktiskaisDarbs3.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var list = await _db.Categories
                .Include(c => c.Items)
                .AsNoTracking()
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Categories.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kategorija pievienota.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid) return View(model);

            try
            {
                _db.Categories.Update(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Kategorija atjaunināta.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Categories.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kļūda: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cat = await _db.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Kategorija dzēsta.";
            return RedirectToAction(nameof(Index));
        }
    }
}
