using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        public OrdersController(IOrderRepository orders) => _orders = orders;

        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            var uid = HttpContext.RequireUserId();
            var (items, total) = await _orders.ListAsync(uid, q, page, pageSize);

            ViewBag.Query = q; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var uid = HttpContext.RequireUserId();
            var order = await _orders.GetAsync(uid, id);
            return View(order);
        }
    }
}
