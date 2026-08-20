using CityCrm.Api.Data;
using CityCrm.Api.Entities;
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
    }
}