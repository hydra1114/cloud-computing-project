namespace InventoryApi.Models
{
    public class ItemLocation
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Item Item { get; set; } = null!;
        public Location Location { get; set; } = null!;
    }
}
