using Microsoft.AspNetCore.Mvc;
using PraktiskaisDarbs3.Data;
using PraktiskaisDarbs3.Models;
using Microsoft.EntityFrameworkCore;

namespace PraktiskaisDarbs3.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string? categoryName, string? itemName, string? orderItem)
        {
            var vm = new QueriesViewModel();

            vm.LowestStockItem = _db.Items
                .Include(i => i.Category)
                .OrderBy(i => i.Quantity)
                .FirstOrDefault();

            vm.HighestStockItem = _db.Items
                .Include(i => i.Category)
                .OrderByDescending(i => i.Quantity)
                .FirstOrDefault();

            vm.NewestOrder = _db.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .FirstOrDefault();

            vm.CategoryNameInput = categoryName;
            vm.ItemNameInput = itemName;
            vm.OrderItemSearch = orderItem;

            if (!string.IsNullOrEmpty(categoryName))
            {
                var categoryNameLower = categoryName.ToLower();
                vm.ItemsByCategory = _db.Items
                    .Where(i => i.Category.Name.ToLower().Contains(categoryNameLower))
                    .Include(i => i.Category)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(itemName))
            {
                var itemNameLower = itemName.ToLower();
                vm.ItemsByNameSearch = _db.Items
                    .Where(i => i.Name.ToLower().Contains(itemNameLower))
                    .Include(i => i.Category)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(orderItem))
            {
                var orderItemLower = orderItem.ToLower();
                var allOrders = _db.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                    .ThenInclude(i => i.Category)
                    .ToList();
                
                vm.OrdersByItemName = allOrders
                    .Where(o => o.OrderItems != null && o.OrderItems.Any(oi => 
                        oi.Item != null && 
                        oi.Item.Name != null && 
                        oi.Item.Name.ToLower().Contains(orderItemLower)))
                    .ToList();
            }

            return View(vm);
        }
    }
}
