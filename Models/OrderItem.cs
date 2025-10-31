using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        [Range(1, 1_000_000)]
        public int Qty { get; set; }

        [Range(0.01, 1_000_000)]
        public decimal Price { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        // Convenience for views
        public string? ProductName { get; set; }
        public string? Category { get; set; }
    }
}
