using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public interface IOrderRepository
    {
        Task<(IEnumerable<Order> Items, int Total)> ListAsync(int userId, string? q, int page, int pageSize);
        Task<Order> GetAsync(int userId, int orderId);
        Task<int> CreateOrderAsync(int userId, string orderNo);
        Task AddItemAsync(int userId, int orderId, int productId, int qty, decimal price);
        Task<bool> DeleteAsync(int userId, int orderId);
        Task<bool> DeleteItemAsync(int userId, int orderId, int itemId);
    }
}
