namespace BackEnd_FLOWER_SHOP.DTOs
{
    public class OrderItemDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long Quantity { get; set; }
        public int Price { get; set; } // Adjust type if needed based on OrderItem.cs
        public string Name { get; set; }
    }
}