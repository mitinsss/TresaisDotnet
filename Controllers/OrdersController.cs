using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PraktiskaisDarbs3.Data;
using PraktiskaisDarbs3.Models;

namespace PraktiskaisDarbs3.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        public OrdersController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Create()
        {
            var items = await _db.Items
                .Include(i => i.Category)
                .AsNoTracking()
                .ToListAsync();
            
            ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");
            return View(new OrderCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (model.OrderItems == null || !model.OrderItems.Any())
            {
                ModelState.AddModelError("", "Jāpievieno vismaz viena prece.");
            }
            else
            {
                for (int i = 0; i < model.OrderItems.Count; i++)
                {
                    var orderItem = model.OrderItems[i];
                    if (orderItem.ItemId.HasValue && orderItem.ItemId.Value > 0)
                    {
                        var item = await _db.Items.FindAsync(orderItem.ItemId.Value);
                        if (item == null)
                        {
                            ModelState.AddModelError($"OrderItems[{i}].ItemId", "Izvēlētā prece nepastāv.");
                        }
                        else if (orderItem.Amount <= 0)
                        {
                            ModelState.AddModelError($"OrderItems[{i}].Amount", "Daudzumam jābūt lielākam par 0.");
                        }
                        else if (item.Quantity < orderItem.Amount)
                        {
                            ModelState.AddModelError($"OrderItems[{i}].Amount", $"Nav pietiekami daudz preces '{item.Name}' krājumā. Pieejams: {item.Quantity}");
                        }
                    }
                }

                var validItems = model.OrderItems.Where(oi => oi.ItemId.HasValue && oi.ItemId.Value > 0 && oi.Amount > 0).ToList();
                if (!validItems.Any())
                {
                    ModelState.AddModelError("", "Jāpievieno vismaz viena derīga prece.");
                }
            }

            if (!ModelState.IsValid)
            {
                var items = await _db.Items
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");
                return View(model);
            }

            try
            {
                var order = new Order
                {
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var orderItemInput in (model.OrderItems ?? new List<OrderItemInput>()).Where(oi => oi.ItemId.HasValue && oi.ItemId.Value > 0 && oi.Amount > 0))
                {
                    var itemId = orderItemInput.ItemId!.Value;
                    var item = await _db.Items.FindAsync(itemId);
                    if (item != null)
                    {
                        item.Quantity -= orderItemInput.Amount;
                        order.OrderItems.Add(new OrderItem
                        {
                            ItemId = orderItemInput.ItemId.Value,
                            Amount = orderItemInput.Amount
                        });
                    }
                }

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Pasūtījums pievienots.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kļūda: {ex.Message}");
                var items = await _db.Items
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var items = await _db.Items
                .Include(i => i.Category)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");

            var viewModel = new OrderCreateViewModel
            {
                OrderItems = order.OrderItems.Select(oi => new OrderItemInput
                {
                    ItemId = oi.ItemId,
                    Amount = oi.Amount
                }).ToList()
            };

            ViewBag.OrderId = order.Id;
            ViewBag.CreatedAt = order.CreatedAt;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderCreateViewModel model, DateTime createdAt)
        {
            if (model.OrderItems == null || !model.OrderItems.Any())
            {
                ModelState.AddModelError("", "Jāpievieno vismaz viena prece.");
            }
            else
            {
                for (int i = 0; i < model.OrderItems.Count; i++)
                {
                    var orderItem = model.OrderItems[i];
                    if (orderItem.ItemId.HasValue && orderItem.ItemId.Value > 0)
                    {
                        var item = await _db.Items.FindAsync(orderItem.ItemId.Value);
                        if (item == null)
                        {
                            ModelState.AddModelError($"OrderItems[{i}].ItemId", "Izvēlētā prece nepastāv.");
                        }
                        else if (orderItem.Amount <= 0)
                        {
                            ModelState.AddModelError($"OrderItems[{i}].Amount", "Daudzumam jābūt lielākam par 0.");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var items = await _db.Items
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");
                ViewBag.OrderId = id;
                ViewBag.CreatedAt = createdAt;
                return View(model);
            }

            try
            {
                var order = await _db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return NotFound();

                foreach (var oldOrderItem in order.OrderItems)
                {
                    var oldItem = await _db.Items.FindAsync(oldOrderItem.ItemId);
                    if (oldItem != null)
                    {
                        oldItem.Quantity += oldOrderItem.Amount;
                    }
                }

                _db.OrderItems.RemoveRange(order.OrderItems);

                order.OrderItems.Clear();
                foreach (var orderItemInput in (model.OrderItems ?? new List<OrderItemInput>()).Where(oi => oi.ItemId.HasValue && oi.ItemId.Value > 0 && oi.Amount > 0))
                {
                    var itemId = orderItemInput.ItemId!.Value;
                    var item = await _db.Items.FindAsync(itemId);
                    if (item != null)
                    {
                        if (item.Quantity < orderItemInput.Amount)
                        {
                            ModelState.AddModelError("", $"Nav pietiekami daudz preces '{item.Name}' krājumā. Pieejams: {item.Quantity}");
                            var items = await _db.Items
                                .Include(i => i.Category)
                                .AsNoTracking()
                                .ToListAsync();
                            ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");
                            ViewBag.OrderId = id;
                            ViewBag.CreatedAt = createdAt;
                            return View(model);
                        }

                        item.Quantity -= orderItemInput.Amount;
                        order.OrderItems.Add(new OrderItem
                        {
                            ItemId = orderItemInput.ItemId.Value,
                            Amount = orderItemInput.Amount
                        });
                    }
                }

                order.CreatedAt = createdAt;
                await _db.SaveChangesAsync();

                TempData["Success"] = "Pasūtījums atjaunināts.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kļūda: {ex.Message}");
                var items = await _db.Items
                    .Include(i => i.Category)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Items = new SelectList(items, "Id", "Name", null, "Category.Name");
                ViewBag.OrderId = id;
                ViewBag.CreatedAt = createdAt;
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            foreach (var orderItem in order.OrderItems)
            {
                var item = await _db.Items.FindAsync(orderItem.ItemId);
                if (item != null)
                {
                    item.Quantity += orderItem.Amount;
                }
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Pasūtījums dzēsts.";
            return RedirectToAction(nameof(Index));
        }
    }
}
