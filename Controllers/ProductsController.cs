using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
    [Authorize] // cookie-protected
    public class ProductsController : Controller
    {
        private readonly IProductRepository _repo;
        public ProductsController(IProductRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.RequireUserId();
            var items = await _repo.GetAllAsync(userId, q, page, pageSize);
            var total = await _repo.CountAsync(userId, q);
            ViewBag.Query = q; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
            return View(items);
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
    }
}