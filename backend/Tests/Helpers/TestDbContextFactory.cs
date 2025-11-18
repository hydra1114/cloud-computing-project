using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Models;

namespace InventoryApi.Tests.Helpers
{
    public class TestDbContextFactory
    {
        public static InventoryContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<InventoryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new InventoryContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        public static InventoryContext CreateContextWithData()
        {
            var context = CreateContext();

            // Add test items
            var items = new[]
            {
                new Item { Id = 1, Name = "Test Item 1", Price = 10.50m, SKU = "SKU001", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = 2, Name = "Test Item 2", Price = 25.99m, SKU = "SKU002", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = 3, Name = "Test Item 3", Price = 5.00m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Items.AddRange(items);

            // Add test locations
            var locations = new[]
            {
                new Location { Id = 1, Name = "Factory", LocationType = "Factory", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Location { Id = 2, Name = "Home", LocationType = "Home", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Location { Id = 3, Name = "Warehouse", LocationType = "Warehouse", ParentLocationId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Locations.AddRange(locations);

            // Add test item locations
            var itemLocations = new[]
            {
                new ItemLocation { Id = 1, ItemId = 1, LocationId = 1, Quantity = 100, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ItemLocation { Id = 2, ItemId = 2, LocationId = 2, Quantity = 50, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.ItemLocations.AddRange(itemLocations);

            context.SaveChanges();

            return context;
        }
    }
}
