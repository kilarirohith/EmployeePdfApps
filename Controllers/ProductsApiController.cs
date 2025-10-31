using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Dtos;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
   
    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly IProductRepository _repo;
        public ProductsApiController(IProductRepository repo) => _repo = repo;

      
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProductReadDto>), 200)]
        public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var uid = HttpContext.RequireUserId();
            var rows = await _repo.GetAllAsync(uid, q, page, pageSize);
            var total = await _repo.CountAsync(uid, q);

            var dto = new PagedResult<ProductReadDto>
            {
                Items = rows.Select(p => new ProductReadDto { Id = p.Id, Name = p.Name, Price = p.Price, Category = p.Category }),
                TotalCount = total, Page = page, PageSize = pageSize
            };
            return Ok(dto);
        }

        /// <summary>Create product.</summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ProductReadDto), 201)]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            var uid = HttpContext.RequireUserId();
            var id = await _repo.CreateAsync(uid, new Product { Name = dto.Name, Price = dto.Price, Category = dto.Category });
            var p = await _repo.GetByIdAsync(uid, id);
            return CreatedAtAction(nameof(Get), new { id = p.Id }, new ProductReadDto { Id = p.Id, Name = p.Name, Price = p.Price, Category = p.Category });
        }

        /// <summary>Get product by id.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(int id)
        {
            var uid = HttpContext.RequireUserId();
            var p = await _repo.GetByIdAsync(uid, id);
            return Ok(new ProductReadDto { Id = p.Id, Name = p.Name, Price = p.Price, Category = p.Category });
        }

        /// <summary>Update product.</summary>
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateDto dto)
        {
            var uid = HttpContext.RequireUserId();
            await _repo.UpdateAsync(uid, new Product { Id = id, Name = dto.Name, Price = dto.Price, Category = dto.Category });
            return NoContent();
        }

        /// <summary>Delete product.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = HttpContext.RequireUserId();
            await _repo.DeleteAsync(uid, id);
            return NoContent();
        }
    }
}

