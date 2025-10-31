using Microsoft.AspNetCore.Authorization;
using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace EmployeeCrudPdf.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _repo;
        public EmployeesController(IEmployeeRepository repo) => _repo = repo;

        // GET: /Employees?q=&page=&pageSize=&useLinq=
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
                    query = query.Where(e =>
                        (e.Name ?? "").ToLower().Contains(kw) ||
                        (e.Department ?? "").ToLower().Contains(kw) ||
                        (e.Email ?? "").ToLower().Contains(kw));
                }

                var total = query.Count();
                var items = query
                    .OrderByDescending(e => e.Id)
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

        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.RequireUserId();
            var emp = await _repo.GetByIdAsync(userId, id);
            return View(emp);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee emp)
        {
            if (!ModelState.IsValid) return View(emp);
            var userId = HttpContext.RequireUserId();
            var id = await _repo.CreateAsync(userId, emp);
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.RequireUserId();
            var emp = await _repo.GetByIdAsync(userId, id);
            return View(emp);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee emp)
        {
            if (id != emp.Id) return BadRequest();
            if (!ModelState.IsValid) return View(emp);
            var userId = HttpContext.RequireUserId();
            await _repo.UpdateAsync(userId, emp);
            return RedirectToAction(nameof(Details), new { id = emp.Id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.RequireUserId();
            var emp = await _repo.GetByIdAsync(userId, id);
            return View(emp);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.RequireUserId();
            await _repo.DeleteAsync(userId, id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var userId = HttpContext.RequireUserId();
            var emp = await _repo.GetByIdAsync(userId, id);
            return new ViewAsPdf("DetailsPdf", emp)
            {
                FileName = $"Employee_{emp.Id}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                PageMargins = new Margins(18, 18, 18, 18),
                CustomSwitches = "--disable-smart-shrinking"
            };
        }
    }
}
