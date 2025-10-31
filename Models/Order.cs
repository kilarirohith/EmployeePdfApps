using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string OrderNo { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }

        public List<OrderItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Price * i.Qty);
    }
}
