using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Services
{
    public interface IJwtService
    {
        string CreateToken(User user);
    }
}
