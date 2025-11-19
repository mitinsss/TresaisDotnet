using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PraktiskaisDarbs3.Data;
using PraktiskaisDarbs3.Models;

namespace PraktiskaisDarbs3.Controllers
{
    public class ItemsController : Controller
    {
        private readonly AppDbContext _db;
        public ItemsController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var items = await _db.Items
                .Include(i => i.Category)
                .OrderByDescending(i => i.Id)
                .AsNoTracking()
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item model)
        {
            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Kategorija ir obligāta.");
            }
            else
            {
                var categoryExists = await _db.Categories.AnyAsync(c => c.Id == model.CategoryId);
                if (!categoryExists)
                {
                    ModelState.AddModelError("CategoryId", "Izvēlētā kategorija neeksistē.");
                }
            }

            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
                return View(model);
            }

            try
            {
                _db.Items.Add(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Prece pievienota.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kļūda: {ex.Message}");
                ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();

            ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item model)
        {
            if (id != model.Id) return NotFound();

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Kategorija ir obligāta.");
            }
            else
            {
                var categoryExists = await _db.Categories.AnyAsync(c => c.Id == model.CategoryId);
                if (!categoryExists)
                {
                    ModelState.AddModelError("CategoryId", "Izvēlētā kategorija neeksistē.");
                }
            }

            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
                return View(model);
            }

            try
            {
                _db.Items.Update(model);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Prece atjaunināta.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Items.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kļūda: {ex.Message}");
                ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var it = await _db.Items.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);
            if (it == null) return NotFound();
            return View(it);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var it = await _db.Items.FindAsync(id);
            if (it == null) return NotFound();

            _db.Items.Remove(it);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Prece dzēsta.";
            return RedirectToAction(nameof(Index));
        }
    }
}
