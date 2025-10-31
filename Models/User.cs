using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(80)] 
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)] 
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
