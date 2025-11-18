using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryApi.Controllers;
using InventoryApi.Data;
using InventoryApi.Models;
using InventoryApi.Tests.Helpers;

namespace InventoryApi.Tests.Controllers
{
    public class ItemsControllerTests
    {
        [Fact]
        public async Task GetItems_ReturnsAllItems()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);

            // Act
            var result = await controller.GetItems();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Item>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<Item>>(actionResult.Value);
            Assert.Equal(3, items.Count());
        }

        [Fact]
        public async Task GetItem_WithValidId_ReturnsItem()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);

            // Act
            var result = await controller.GetItem(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Item>>(result);
            var item = Assert.IsType<Item>(actionResult.Value);
            Assert.Equal(1, item.Id);
            Assert.Equal("Test Item 1", item.Name);
        }

        [Fact]
        public async Task GetItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);

            // Act
            var result = await controller.GetItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateItem_WithValidData_CreatesItem()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContext();
            var controller = new ItemsController(context);
            var newItem = new Item
            {
                Name = "New Item",
                Price = 15.99m,
                SKU = "NEW001",
                Description = "Test description"
            };

            // Act
            var result = await controller.CreateItem(newItem);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Item>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var item = Assert.IsType<Item>(createdResult.Value);
            Assert.Equal("New Item", item.Name);
            Assert.Equal(15.99m, item.Price);
            Assert.True(item.Id > 0);
        }

        [Fact]
        public async Task UpdateItem_WithValidData_UpdatesItem()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);
            var updatedItem = new Item
            {
                Id = 1,
                Name = "Updated Item",
                Price = 99.99m,
                SKU = "UPDATED001"
            };

            // Act
            var result = await controller.UpdateItem(1, updatedItem);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var item = await context.Items.FindAsync(1);
            Assert.Equal("Updated Item", item!.Name);
            Assert.Equal(99.99m, item.Price);
        }

        [Fact]
        public async Task UpdateItem_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);
            var updatedItem = new Item { Id = 2, Name = "Updated Item", Price = 99.99m };

            // Act
            var result = await controller.UpdateItem(1, updatedItem);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteItem_WithValidId_DeletesItem()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);

            // Act
            var result = await controller.DeleteItem(3);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var item = await context.Items.FindAsync(3);
            Assert.Null(item);
        }

        [Fact]
        public async Task DeleteItem_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemsController(context);

            // Act
            var result = await controller.DeleteItem(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
