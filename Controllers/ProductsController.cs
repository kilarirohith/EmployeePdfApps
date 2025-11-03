using System.Linq;
using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
    [Authorize] git add;
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repo;
        private readonly IOrderRepository _orders;

        public ProductsController(IProductRepository repo, IOrderRepository orders)
        {
            _repo = repo;
            _orders = orders;
        }

        // GET: /Products?q=...&page=1&pageSize=10
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10, bool useLinq = false)
        {
            var userId = HttpContext.RequireUserId();

            if (useLinq)
            {
                var all = await _repo.GetAllAsync(userId, keyword: null, page: null, pageSize: null);
                var kw = (q ?? "").Trim().ToLowerInvariant();

                var query = all.AsQueryable();
                if (!string.IsNullOrWhiteSpace(kw))
                {
                    query = query.Where(p =>
                        (p.Name ?? "").ToLower().Contains(kw) ||
                        (p.Category ?? "").ToLower().Contains(kw));
                }

                var total = query.Count();
                var items = query
                    .OrderByDescending(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Query = q; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total; ViewBag.UseLinq = true;
                return View(items);
            }
            else
            {
                var items = await _repo.GetAllAsync(userId, q, page, pageSize);
                var total = await _repo.CountAsync(userId, q);
                ViewBag.Query = q; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total; ViewBag.UseLinq = false;
                return View(items);
            }
        }

        public IActionResult Create() => View(new Product());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product p)
        {
            if (!ModelState.IsValid) return View(p);
            var userId = HttpContext.RequireUserId();
            p.Id = await _repo.CreateAsync(userId, p);
            TempData["ok"] = "Product created.";
            return RedirectToAction(nameof(Details), new { id = p.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.RequireUserId();
            var p = await _repo.GetByIdAsync(userId, id);
            return View(p);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.RequireUserId();
            var p = await _repo.GetByIdAsync(userId, id);
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product p)
        {
            if (id != p.Id) return BadRequest();
            if (!ModelState.IsValid) return View(p);
            var userId = HttpContext.RequireUserId();
            await _repo.UpdateAsync(userId, p);
            TempData["ok"] = "Product updated.";
            return RedirectToAction(nameof(Details), new { id = p.Id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.RequireUserId();
            var p = await _repo.GetByIdAsync(userId, id);
            return View(p);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.RequireUserId();
            await _repo.DeleteAsync(userId, id);
            TempData["ok"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- NEW: Add to Order (GET shows form with recent orders) ----------
        [HttpGet]
        public async Task<IActionResult> AddToOrder(int productId)
        {
            var uid = HttpContext.RequireUserId();
            var p = await _repo.GetByIdAsync(uid, productId);

            // fetch a small page of recent orders for dropdown
            var (items, _) = await _orders.ListAsync(uid, q: null, page: 1, pageSize: 10);
            var vm = new AddToOrderViewModel
            {
                ProductId = productId,
                ProductName = p.Name,
                RecentOrders = items.Select(o => (o.Id, o.OrderNo))
            };
            return View(vm);
        }

        // ---------- NEW: Add to Order (POST does the work) ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToOrder(AddToOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // repopulate recent orders for re-render
                var uid = HttpContext.RequireUserId();
                var (items, _) = await _orders.ListAsync(uid, q: null, page: 1, pageSize: 10);
                vm.RecentOrders = items.Select(o => (o.Id, o.OrderNo));
                return View(vm);
            }

            var userId = HttpContext.RequireUserId();

            // ensure product exists and get current price
            var p = await _repo.GetByIdAsync(userId, vm.ProductId);

            int orderId;
            if (vm.OrderId.HasValue && vm.OrderId.Value > 0)
            {
                // ensure order exists (GetAsync will throw if not)
                var _ = await _orders.GetAsync(userId, vm.OrderId.Value);
                orderId = vm.OrderId.Value;
            }
            else
            {
                // create a fresh order
                var orderNo = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
                orderId = await _orders.CreateOrderAsync(userId, orderNo);
            }

            await _orders.AddItemAsync(userId, orderId, p.Id, vm.Qty, price: p.Price);

            TempData["ok"] = "Item added to order.";
            return RedirectToAction("Details", "Orders", new { id = orderId });
        }
    }
}
