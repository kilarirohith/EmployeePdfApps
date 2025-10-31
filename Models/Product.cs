using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(120)] 
        public string Name { get; set; } = string.Empty;

        [Range(0, 1_000_000)] 
        public decimal Price { get; set; }

        [StringLength(200)] 
        public string? Category { get; set; }

        public int UserId { get; set; }
    }
}
