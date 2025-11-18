using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Models;

namespace InventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemLocationsController : ControllerBase
    {
        private readonly InventoryContext _context;

        public ItemLocationsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/ItemLocations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemLocation>>> GetItemLocations()
        {
            return await _context.ItemLocations
                .Include(il => il.Item)
                .Include(il => il.Location)
                .ToListAsync();
        }

        // GET: api/ItemLocations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemLocation>> GetItemLocation(int id)
        {
            var itemLocation = await _context.ItemLocations
                .Include(il => il.Item)
                .Include(il => il.Location)
                .FirstOrDefaultAsync(il => il.Id == id);

            if (itemLocation == null)
            {
                return NotFound();
            }

            return itemLocation;
        }

        // GET: api/ItemLocations/ByLocation/5
        [HttpGet("ByLocation/{locationId}")]
        public async Task<ActionResult<IEnumerable<ItemLocation>>> GetItemLocationsByLocation(int locationId)
        {
            return await _context.ItemLocations
                .Include(il => il.Item)
                .Include(il => il.Location)
                .Where(il => il.LocationId == locationId)
                .ToListAsync();
        }

        // GET: api/ItemLocations/ByItem/5
        [HttpGet("ByItem/{itemId}")]
        public async Task<ActionResult<IEnumerable<ItemLocation>>> GetItemLocationsByItem(int itemId)
        {
            return await _context.ItemLocations
                .Include(il => il.Item)
                .Include(il => il.Location)
                .Where(il => il.ItemId == itemId)
                .ToListAsync();
        }

        // POST: api/ItemLocations
        [HttpPost]
        public async Task<ActionResult<ItemLocation>> CreateItemLocation(ItemLocation itemLocation)
        {
            // Check if item exists
            var itemExists = await _context.Items.AnyAsync(i => i.Id == itemLocation.ItemId);
            if (!itemExists)
            {
                return BadRequest("Item does not exist.");
            }

            // Check if location exists
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == itemLocation.LocationId);
            if (!locationExists)
            {
                return BadRequest("Location does not exist.");
            }

            // Check if this item-location combination already exists
            var exists = await _context.ItemLocations
                .AnyAsync(il => il.ItemId == itemLocation.ItemId && il.LocationId == itemLocation.LocationId);

            if (exists)
            {
                return BadRequest("This item is already assigned to this location.");
            }

            itemLocation.CreatedAt = DateTime.UtcNow;
            itemLocation.UpdatedAt = DateTime.UtcNow;

            _context.ItemLocations.Add(itemLocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemLocation), new { id = itemLocation.Id }, itemLocation);
        }

        // PUT: api/ItemLocations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItemLocation(int id, ItemLocation itemLocation)
        {
            if (id != itemLocation.Id)
            {
                return BadRequest();
            }

            var existingItemLocation = await _context.ItemLocations.FindAsync(id);
            if (existingItemLocation == null)
            {
                return NotFound();
            }

            existingItemLocation.Quantity = itemLocation.Quantity;
            existingItemLocation.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemLocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ItemLocations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemLocation(int id)
        {
            var itemLocation = await _context.ItemLocations.FindAsync(id);
            if (itemLocation == null)
            {
                return NotFound();
            }

            _context.ItemLocations.Remove(itemLocation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemLocationExists(int id)
        {
            return _context.ItemLocations.Any(e => e.Id == id);
        }
    }
}
