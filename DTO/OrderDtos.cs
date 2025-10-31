using System.ComponentModel.DataAnnotations;

namespace EmployeeCrudPdf.Dtos
{
    /// <summary>Create order with items. If price is omitted, current product price is used.</summary>
    public class OrderCreateDto
    {
        public List<OrderCreateItemDto> Items { get; set; } = new();
    }

    public class OrderCreateItemDto
    {
        [Required] public int ProductId { get; set; }
        [Range(1, 1_000_000)] public int Qty { get; set; }
        /// <summary>Optional override price; if 0 or omitted, system uses product price.</summary>
        public decimal? Price { get; set; }
    }

    /// <summary>Order read shape.</summary>
    public class OrderReadDto
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemReadDto> Items { get; set; } = new();
    }

    public class OrderItemReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Qty * Price;
    }

    /// <summary>List row for order lists.</summary>
    public class OrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public int ItemsCount { get; set; }
    }
}
