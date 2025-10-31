using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<int> CreateAsync(User u);
    }
}
