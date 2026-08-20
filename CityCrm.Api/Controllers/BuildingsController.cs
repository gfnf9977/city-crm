using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BuildingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuildings()
        {
            var buildings = await _context.Buildings.Include(b => b.Premises).ToListAsync();
            var writer = new NetTopologySuite.IO.GeoJsonWriter();

            foreach (var b in buildings)
            {
                b.Lat = b.Location != null ? b.Location.Centroid.Y : 0;
                b.Lng = b.Location != null ? b.Location.Centroid.X : 0;

                if (b.Location != null && b.Location.GeometryType != "Point")
                {
                    b.GeoJson = writer.Write(b.Location);
                }
            }

            return Ok(buildings);
        }

        [HttpGet("osm-contour")]
        public async Task<IActionResult> GetOsmContour([FromServices] CityCrm.Api.Services.OsmService osmService, [FromQuery] string street, [FromQuery] string number)
        {
            var (geometry, errorMessage) = await osmService.GetBuildingGeometryAsync("Чернігів", street, number);
            
            if (errorMessage == "RATE_LIMIT")
                return StatusCode(429, new { message = "RATE_LIMIT" });
                
            if (errorMessage == "ERROR")
                return StatusCode(502, new { message = "OSM_SERVER_DOWN" });

            if (geometry == null)
                return NotFound();

            var writer = new NetTopologySuite.IO.GeoJsonWriter();
            return Ok(writer.Write(geometry));
        }

        [HttpPost]
        public async Task<ActionResult<Building>> CreateBuilding(Building building)
        {
            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            if (!string.IsNullOrEmpty(building.GeoJson))
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                building.Location = reader.Read<NetTopologySuite.Geometries.Geometry>(building.GeoJson);
            }
            else if (building.Lat != 0 && building.Lng != 0)
            {
                building.Location = factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(building.Lng, building.Lat));
            }
            else
            {
                return BadRequest("Не вказано розташування будівлі.");
            }

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBuildings), new { id = building.Id }, building);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, Building building)
        {
            if (id != building.Id) return BadRequest("ID не співпадає.");

            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            if (!string.IsNullOrEmpty(building.GeoJson))
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                building.Location = reader.Read<NetTopologySuite.Geometries.Geometry>(building.GeoJson);
            }
            else if (building.Lat != 0 && building.Lng != 0)
            {
                building.Location = factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(building.Lng, building.Lat));
            }
            else
            {
                return BadRequest("Не вказано розташування будівлі.");
            }

            _context.Entry(building).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Buildings.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return NotFound();

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}