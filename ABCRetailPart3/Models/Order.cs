namespace ABCRetailPart3.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } 
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "PENDING";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string ShippingAddress { get; set; } = string.Empty;
        public string OrderItemsJson { get; set; } = string.Empty;

        // Navigation property
        public ApplicationUser Customer { get; set; } = null!;
    }
}