using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(int userId, string? keyword = null, int? page = null, int? pageSize = null);
    Task<int> CountAsync(int userId, string? keyword = null);
    Task<Product> GetByIdAsync(int userId, int id);
    Task<int> CreateAsync(int userId, Product p);
    Task<bool> UpdateAsync(int userId, Product p);
    Task<bool> DeleteAsync(int userId, int id);
}

}
