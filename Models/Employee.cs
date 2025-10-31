using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
         public string Name { get; set; } = string.Empty;

        [Required, StringLength(100)] 
        public string Department { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(255)]
         public string Email { get; set; } = string.Empty;

        [Range(0, 1_000_000)] 
        public decimal Salary { get; set; }

        public int UserId { get; set; } 
    }
}
