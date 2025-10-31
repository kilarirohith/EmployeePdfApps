using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Dtos
{
    /// <summary>Payload to create or update a product.</summary>
    public class ProductCreateDto
    {
        [Required, StringLength(120)] public string Name { get; set; } = "";
        [Range(0.01, 1_000_000)] public decimal Price { get; set; }
        [StringLength(200)] public string? Category { get; set; }
    }

    /// <summary>Product read shape.</summary>
    public class ProductReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? Category { get; set; }
    }
}
