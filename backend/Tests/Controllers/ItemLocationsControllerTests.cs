using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using InventoryApi.Controllers;
using InventoryApi.Data;
using InventoryApi.Models;
using InventoryApi.Tests.Helpers;

namespace InventoryApi.Tests.Controllers
{
    public class ItemLocationsControllerTests
    {
        [Fact]
        public async Task GetItemLocations_ReturnsAllItemLocations()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);

            // Act
            var result = await controller.GetItemLocations();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ItemLocation>>>(result);
            var itemLocations = Assert.IsAssignableFrom<IEnumerable<ItemLocation>>(actionResult.Value);
            Assert.Equal(2, itemLocations.Count());
        }

        [Fact]
        public async Task GetItemLocationsByLocation_ReturnsCorrectItems()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);

            // Act
            var result = await controller.GetItemLocationsByLocation(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ItemLocation>>>(result);
            var itemLocations = Assert.IsAssignableFrom<IEnumerable<ItemLocation>>(actionResult.Value);
            Assert.Single(itemLocations);
            Assert.All(itemLocations, il => Assert.Equal(1, il.LocationId));
        }

        [Fact]
        public async Task GetItemLocationsByItem_ReturnsCorrectLocations()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);

            // Act
            var result = await controller.GetItemLocationsByItem(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ItemLocation>>>(result);
            var itemLocations = Assert.IsAssignableFrom<IEnumerable<ItemLocation>>(actionResult.Value);
            Assert.Single(itemLocations);
            Assert.All(itemLocations, il => Assert.Equal(1, il.ItemId));
        }

        [Fact]
        public async Task CreateItemLocation_WithValidData_CreatesItemLocation()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);
            var newItemLocation = new ItemLocation
            {
                ItemId = 3,
                LocationId = 1,
                Quantity = 75
            };

            // Act
            var result = await controller.CreateItemLocation(newItemLocation);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ItemLocation>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var itemLocation = Assert.IsType<ItemLocation>(createdResult.Value);
            Assert.Equal(3, itemLocation.ItemId);
            Assert.Equal(1, itemLocation.LocationId);
            Assert.Equal(75, itemLocation.Quantity);
        }

        [Fact]
        public async Task CreateItemLocation_WithNonExistentItem_ReturnsBadRequest()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);
            var newItemLocation = new ItemLocation
            {
                ItemId = 999,
                LocationId = 1,
                Quantity = 75
            };

            // Act
            var result = await controller.CreateItemLocation(newItemLocation);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ItemLocation>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateItemLocation_WithNonExistentLocation_ReturnsBadRequest()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);
            var newItemLocation = new ItemLocation
            {
                ItemId = 1,
                LocationId = 999,
                Quantity = 75
            };

            // Act
            var result = await controller.CreateItemLocation(newItemLocation);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ItemLocation>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task CreateItemLocation_WithDuplicateItemLocation_ReturnsBadRequest()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);
            var duplicateItemLocation = new ItemLocation
            {
                ItemId = 1,
                LocationId = 1,
                Quantity = 50
            };

            // Act
            var result = await controller.CreateItemLocation(duplicateItemLocation);

            // Assert
            var actionResult = Assert.IsType<ActionResult<ItemLocation>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task UpdateItemLocation_WithValidData_UpdatesQuantity()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);
            var updatedItemLocation = new ItemLocation
            {
                Id = 1,
                ItemId = 1,
                LocationId = 1,
                Quantity = 200
            };

            // Act
            var result = await controller.UpdateItemLocation(1, updatedItemLocation);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var itemLocation = await context.ItemLocations.FindAsync(1);
            Assert.Equal(200, itemLocation!.Quantity);
        }

        [Fact]
        public async Task DeleteItemLocation_WithValidId_DeletesItemLocation()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);

            // Act
            var result = await controller.DeleteItemLocation(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var itemLocation = await context.ItemLocations.FindAsync(1);
            Assert.Null(itemLocation);
        }

        [Fact]
        public async Task DeleteItemLocation_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateContextWithData();
            var controller = new ItemLocationsController(context);

            // Act
            var result = await controller.DeleteItemLocation(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
