using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public interface IUserRepository
    {
         Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<int> CreateAsync(User user);
    }
}
