
using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync(int userId, string? keyword = null, int? page = null, int? pageSize = null);
        Task<int> CountAsync(int userId, string? keyword = null);
        Task<Employee> GetByIdAsync(int userId, int id);
        Task<int> CreateAsync(int userId, Employee emp);
        Task<bool> UpdateAsync(int userId, Employee emp);
        Task<bool> DeleteAsync(int userId, int id);
    }
}
