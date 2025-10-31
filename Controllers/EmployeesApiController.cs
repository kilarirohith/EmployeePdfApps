using EmployeeCrudPdf.Data;
using EmployeeCrudPdf.Dtos;
using EmployeeCrudPdf.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeCrudPdf.Controllers
{
    /// <summary>Employee endpoints (per-user).</summary>
    [ApiController]
    [Authorize]
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
                Items = rows.Select(e => new EmployeeReadDto
                {
                    Id = e.Id, Name = e.Name, Department = e.Department, Email = e.Email, Salary = e.Salary
                }),
                TotalCount = total, Page = page, PageSize = pageSize
            };
            return Ok(dto);
        }

       
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(EmployeeReadDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
        {
            var uid = HttpContext.RequireUserId();
            var id = await _repo.CreateAsync(uid, new Employee
            {
                Name = dto.Name, Department = dto.Department, Email = dto.Email, Salary = dto.Salary
            });

            var created = await _repo.GetByIdAsync(uid, id);
            var read = new EmployeeReadDto { Id = created.Id, Name = created.Name, Department = created.Department, Email = created.Email, Salary = created.Salary };
            return CreatedAtAction(nameof(Get), new { id = read.Id }, read);
        }

        
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EmployeeReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(int id)
        {
            var uid = HttpContext.RequireUserId();
            var e = await _repo.GetByIdAsync(uid, id);
            return Ok(new EmployeeReadDto { Id = e.Id, Name = e.Name, Department = e.Department, Email = e.Email, Salary = e.Salary });
        }

        
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeCreateDto dto)
        {
            var uid = HttpContext.RequireUserId();
            await _repo.UpdateAsync(uid, new Employee
            {
                Id = id, Name = dto.Name, Department = dto.Department, Email = dto.Email, Salary = dto.Salary
            });
            return NoContent();
        }

        
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

