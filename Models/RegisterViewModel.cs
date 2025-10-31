using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class RegisterViewModel
    {
        [Required, StringLength(80)] public string Username { get; set; } = "";
        [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
        [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = "";
    }
}
