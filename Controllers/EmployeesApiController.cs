using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Dtos;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [Route("api/employees")]
    public class EmployeesApiController : ControllerBase
    {
        private readonly IEmployeeRepository _repo;
        public EmployeesApiController(IEmployeeRepository repo) => _repo = repo;

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<EmployeeReadDto>), 200)]
        public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var uid = HttpContext.RequireUserId();
            var rows = await _repo.GetAllAsync(uid, q, page, pageSize);
            var total = await _repo.CountAsync(uid, q);
            var dto = new PagedResult<EmployeeReadDto>
            {
                Items = rows.Select(e => new EmployeeReadDto { Id = e.Id, Name = e.Name, Department = e.Department, Email = e.Email, Salary = e.Salary }),
                TotalCount = total, Page = page, PageSize = pageSize
            };
            return Ok(dto);
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(EmployeeReadDto), 201)]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateDto body)
        {
            var uid = HttpContext.RequireUserId();
            var id = await _repo.CreateAsync(uid, new Employee { Name = body.Name, Department = body.Department, Email = body.Email, Salary = body.Salary });
            var e = await _repo.GetByIdAsync(uid, id);
            return CreatedAtAction(nameof(Get), new { id = e.Id },
                new EmployeeReadDto { Id = e.Id, Name = e.Name, Department = e.Department, Email = e.Email, Salary = e.Salary });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EmployeeReadDto), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var uid = HttpContext.RequireUserId();
            var e = await _repo.GetByIdAsync(uid, id);
            return Ok(new EmployeeReadDto { Id = e.Id, Name = e.Name, Department = e.Department, Email = e.Email, Salary = e.Salary });
        }

        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeCreateDto body)
        {
            var uid = HttpContext.RequireUserId();
            await _repo.UpdateAsync(uid, new Employee { Id = id, Name = body.Name, Department = body.Department, Email = body.Email, Salary = body.Salary });
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = HttpContext.RequireUserId();
            await _repo.DeleteAsync(uid, id);
            return NoContent();
        }
    }
}