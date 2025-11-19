using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Models;
using System.Security.Claims;

namespace InventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly InventoryContext _context;

        public LocationsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return await _context.Locations
                .Where(l => l.UserId == userId)
                .Include(l => l.ParentLocation)
                .Include(l => l.ChildLocations)
                .Include(l => l.ItemLocations)
                .ThenInclude(il => il.Item)
                .ToListAsync();
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var location = await _context.Locations
                .Include(l => l.ParentLocation)
                .Include(l => l.ChildLocations)
                .Include(l => l.ItemLocations)
                .ThenInclude(il => il.Item)
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<Location>> CreateLocation(Location location)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            location.UserId = userId;
            location.CreatedAt = DateTime.UtcNow;
            location.UpdatedAt = DateTime.UtcNow;

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var existingLocation = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            
            if (existingLocation == null)
            {
                return NotFound();
            }

            existingLocation.Name = location.Name;
            existingLocation.Description = location.Description;
            existingLocation.Address = location.Address;
            existingLocation.LocationType = location.LocationType;
            existingLocation.ParentLocationId = location.ParentLocationId;
            existingLocation.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
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

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);
            
            if (location == null)
            {
                return NotFound();
            }

            // Check if location has child locations
            var hasChildren = await _context.Locations.AnyAsync(l => l.ParentLocationId == id);
            if (hasChildren)
            {
                return BadRequest("Cannot delete location with child locations.");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
