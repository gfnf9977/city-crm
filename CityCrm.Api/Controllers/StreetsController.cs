using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StreetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Street>>> SearchStreets([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new List<Street>());

            var query = q.ToLower();

            var streets = await _context.Streets
                .Where(s => s.Name.ToLower().Contains(query) ||
                            (s.OldNames != null && s.OldNames.ToLower().Contains(query)))
                .OrderBy(s => s.Name)
                .Take(15)
                .ToListAsync();

            return Ok(streets);
        }

        [HttpGet("geocode")]
        public async Task<IActionResult> GeocodeAddress([FromServices] CityCrm.Api.Services.OsmService osmService, [FromQuery] string street, [FromQuery] string number)
        {
            if (string.IsNullOrWhiteSpace(street)) return BadRequest();
            
            var coords = await osmService.GetAddressCoordinatesAsync("Чернігів", street, number ?? "");
            
            if (coords != null)
            {
                return Ok(new { lat = coords.Value.Lat, lng = coords.Value.Lng });
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Street>>> GetStreets([FromQuery] string? search)
        {
            var query = _context.Streets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(str => 
                    str.Name.ToLower().Contains(s) || 
                    (str.OldNames != null && str.OldNames.ToLower().Contains(s)));
            }

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Street>> GetStreet(int id)
        {
            var street = await _context.Streets.FindAsync(id);
            if (street == null) return NotFound();
            return street;
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPost]
        public async Task<ActionResult<Street>> CreateStreet(Street street)
        {
            _context.Streets.Add(street);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStreet), new { id = street.Id }, street);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStreet(int id, Street street)
        {
            if (id != street.Id) return BadRequest("ID не співпадає.");

            _context.Entry(street).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Streets.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStreet(int id)
        {
            var street = await _context.Streets.FindAsync(id);
            if (street == null) return NotFound();

            _context.Streets.Remove(street);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}