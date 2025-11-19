using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PraktiskaisDarbs3.Data;
using PraktiskaisDarbs3.Models;

namespace PraktiskaisDarbs3.Controllers
{
    public class QueriesController : Controller
    {
        private readonly AppDbContext _db;
        public QueriesController(AppDbContext db) => _db = db;


        public async Task<IActionResult> LowStock()
        {
            var items = await _db.Items
                .Include(i => i.Category)
                .OrderBy(i => i.Quantity)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
            return View(items);
        }


        public async Task<IActionResult> RecentOrders()
        {
            var since = DateTime.UtcNow.AddDays(-7);
            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .Where(o => o.CreatedAt >= since)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.Since = since;
            return View(orders);
        }

        public async Task<IActionResult> ItemCountPerCategory()
        {
            var q = await _db.Categories
                .Select(c => new CategoryCountViewModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    ItemCount = c.Items.Count
                })
                .OrderByDescending(x => x.ItemCount)
                .AsNoTracking()
                .ToListAsync();

            return View(q);
        }

        [HttpGet]
        public IActionResult SearchItems() => View();

        [HttpPost]
        public async Task<IActionResult> SearchItems(string? nameFragment)
        {
            nameFragment ??= "";
            var q = await _db.Items
                .Include(i => i.Category)
                .Where(i => EF.Functions.Like(i.Name.ToLower(), $"%{nameFragment.ToLower()}%"))
                .OrderBy(i => i.Name)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Query = nameFragment;
            return View("SearchItemsResults", q);
        }

        [HttpGet]
        public IActionResult SearchCategories() => View();

        [HttpPost]
        public async Task<IActionResult> SearchCategories(string? nameFragment)
        {
            nameFragment ??= "";
            var q = await _db.Categories
                .Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{nameFragment.ToLower()}%"))
                .Select(c => new { c.Id, c.Name, ItemCount = c.Items.Count })
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Query = nameFragment;
            return View("SearchCategoriesResults", q);
        }

        [HttpGet]
        public IActionResult SearchOrdersByItem() => View();

        [HttpPost]
        public async Task<IActionResult> SearchOrdersByItem(int itemId)
        {
            var q = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .Where(o => o.OrderItems.Any(oi => oi.ItemId == itemId))
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.ItemId = itemId;
            return View("SearchOrdersByItemResults", q);
        }
    }

    public class CategoryCountViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int ItemCount { get; set; }
    }
}
