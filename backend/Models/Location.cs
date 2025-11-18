namespace InventoryApi.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string LocationType { get; set; } = "General"; // Factory, Home, Warehouse, etc.
        public int? ParentLocationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Location? ParentLocation { get; set; }
        public ICollection<Location> ChildLocations { get; set; } = new List<Location>();
        public ICollection<ItemLocation> ItemLocations { get; set; } = new List<ItemLocation>();
    }
}
