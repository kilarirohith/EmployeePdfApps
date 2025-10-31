using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Dtos
{
    /// <summary>Payload to create or update an employee.</summary>
    public class EmployeeCreateDto
    {
        [Required, StringLength(100)] public string Name { get; set; } = "";
        [Required, StringLength(100)] public string Department { get; set; } = "";
        [Required, EmailAddress, StringLength(255)] public string Email { get; set; } = "";
        [Range(0, 1_000_000)] public decimal Salary { get; set; }
    }

    /// <summary>Employee read shape.</summary>
    public class EmployeeReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        public string Email { get; set; } = "";
        public decimal Salary { get; set; }
    }
}

