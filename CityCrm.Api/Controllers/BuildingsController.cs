using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult<IEnumerable<Building>>> GetBuildings(
            [FromQuery] string? search,
            [FromQuery] string? bType,
            [FromQuery] string? condition,
            [FromQuery] string? pType,
            [FromQuery] string? pStatus,
            [FromQuery] string? pOwnership,
            [FromQuery] double? minArea,
            [FromQuery] double? maxArea)
        {
            var query = _context.Buildings.Include(b => b.Street).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(b => 
                    (b.Street != null && b.Street.Name.ToLower().Contains(s)) || 
                    (b.Notes != null && b.Notes.ToLower().Contains(s)) ||
                    b.BuildingNumber.ToString().Contains(s) ||
                    (b.CoopNumber != null && b.CoopNumber.ToLower().Contains(s))
                );
            }

            if (!string.IsNullOrWhiteSpace(bType))
                query = query.Where(b => b.BuildingType == bType);

            if (!string.IsNullOrWhiteSpace(condition))
                query = query.Where(b => b.Condition == condition);

            bool hasPremiseFilter = !string.IsNullOrWhiteSpace(pType) || 
                                    !string.IsNullOrWhiteSpace(pStatus) || 
                                    !string.IsNullOrWhiteSpace(pOwnership) || 
                                    minArea.HasValue || maxArea.HasValue;

            if (hasPremiseFilter)
            {
                query = query.Include(b => b.Premises.Where(p => 
                    (string.IsNullOrWhiteSpace(pType) || p.Type == pType) &&
                    (string.IsNullOrWhiteSpace(pStatus) || p.Status == pStatus) &&
                    (string.IsNullOrWhiteSpace(pOwnership) || p.Ownership == pOwnership) &&
                    (!minArea.HasValue || p.Area >= minArea.Value) &&
                    (!maxArea.HasValue || p.Area <= maxArea.Value)
                ));

                query = query.Where(b => b.Premises.Any(p => 
                    (string.IsNullOrWhiteSpace(pType) || p.Type == pType) &&
                    (string.IsNullOrWhiteSpace(pStatus) || p.Status == pStatus) &&
                    (string.IsNullOrWhiteSpace(pOwnership) || p.Ownership == pOwnership) &&
                    (!minArea.HasValue || p.Area >= minArea.Value) &&
                    (!maxArea.HasValue || p.Area <= maxArea.Value)
                ));
            }
            else
            {
                query = query.Include(b => b.Premises);
            }

            var buildings = await query.OrderByDescending(b => b.Id).ToListAsync();
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

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPost]
        public async Task<ActionResult<Building>> CreateBuilding(Building building)
        {
            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            if (!string.IsNullOrEmpty(building.GeoJson))
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                building.Location = reader.Read<NetTopologySuite.Geometries.Geometry>(building.GeoJson);
                building.Location.SRID = 4326;
            }
            else if (building.Lat != 0 && building.Lng != 0)
            {
                building.Location = factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(building.Lng, building.Lat));
            }
            else
            {
                return BadRequest("Не вказано розташування будівлі.");
            }

            building.Street = null;

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBuildings), new { id = building.Id }, building);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, Building building)
        {
            if (id != building.Id) return BadRequest("ID не співпадає.");

            var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            if (!string.IsNullOrEmpty(building.GeoJson))
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                building.Location = reader.Read<NetTopologySuite.Geometries.Geometry>(building.GeoJson);
                building.Location.SRID = 4326;
            }
            else if (building.Lat != 0 && building.Lng != 0)
            {
                building.Location = factory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(building.Lng, building.Lat));
            }
            else
            {
                return BadRequest("Не вказано розташування будівлі.");
            }

            building.Street = null;

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

        [Authorize(Roles = "GrandAdmin, Admin")]
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