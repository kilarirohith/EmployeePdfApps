using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class LoginViewModel
    {
        [Required] public string UsernameOrEmail { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        public string? ReturnUrl { get; set; }
    }
}