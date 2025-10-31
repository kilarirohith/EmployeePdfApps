using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Dtos;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [Route("api/orders")]
    public class OrdersApiController : ControllerBase
    {
        private readonly IOrderRepository _orders;
        private readonly IProductRepository _products;

        public OrdersApiController(IOrderRepository orders, IProductRepository products)
        {
            _orders = orders; _products = products;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<OrderListItemDto>), 200)]
        public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var uid = HttpContext.RequireUserId();
            var (items, total) = await _orders.ListAsync(uid, q, page, pageSize);

            var dto = new PagedResult<OrderListItemDto>
            {
                Items = items.Select(o => new OrderListItemDto
                {
                    Id = o.Id, OrderNo = o.OrderNo, CreatedAt = o.CreatedAt,
                    Total = o.Total, ItemsCount = o.Items.Count
                }),
                TotalCount = total, Page = page, PageSize = pageSize
            };
            return Ok(dto);
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(OrderReadDto), 201)]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto body)
        {
            var uid = HttpContext.RequireUserId();
            var orderNo = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
            var orderId = await _orders.CreateOrderAsync(uid, orderNo);

            foreach (var it in body.Items)
            {
                var p = await _products.GetByIdAsync(uid, it.ProductId);
                var price = (it.Price.HasValue && it.Price.Value > 0) ? it.Price.Value : p.Price;
                await _orders.AddItemAsync(uid, orderId, it.ProductId, it.Qty, price);
            }

            var order = await _orders.GetAsync(uid, orderId);
            var dto = new OrderReadDto
            {
                Id = order.Id, OrderNo = order.OrderNo, CreatedAt = order.CreatedAt, Total = order.Total,
                Items = order.Items.Select(i => new OrderItemReadDto
                {
                    Id = i.Id, ProductId = i.ProductId, ProductName = i.ProductName, Qty = i.Qty, Price = i.Price
                }).ToList()
            };
            return CreatedAtAction(nameof(Get), new { id = order.Id }, dto);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrderReadDto), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var uid = HttpContext.RequireUserId();
            var order = await _orders.GetAsync(uid, id);
            return Ok(new OrderReadDto
            {
                Id = order.Id, OrderNo = order.OrderNo, CreatedAt = order.CreatedAt, Total = order.Total,
                Items = order.Items.Select(i => new OrderItemReadDto
                {
                    Id = i.Id, ProductId = i.ProductId, ProductName = i.ProductName, Qty = i.Qty, Price = i.Price
                }).ToList()
            });
        }

        [HttpPost("{id:int}/items")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> AddItem(int id, [FromBody] OrderCreateItemDto body)
        {
            var uid = HttpContext.RequireUserId();
            var p = await _products.GetByIdAsync(uid, body.ProductId);
            var price = (body.Price.HasValue && body.Price.Value > 0) ? body.Price.Value : p.Price;
            await _orders.AddItemAsync(uid, id, body.ProductId, body.Qty, price);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = HttpContext.RequireUserId();
            await _orders.DeleteAsync(uid, id);
            return NoContent();
        }

        [HttpDelete("{orderId:int}/items/{itemId:int}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteItem(int orderId, int itemId)
        {
            var uid = HttpContext.RequireUserId();
            await _orders.DeleteItemAsync(uid, orderId, itemId);
            return NoContent();
        }
    }
}