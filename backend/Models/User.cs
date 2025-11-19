namespace InventoryApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}
