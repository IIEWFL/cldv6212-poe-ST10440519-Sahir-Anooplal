namespace ABCRetailPart3.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}