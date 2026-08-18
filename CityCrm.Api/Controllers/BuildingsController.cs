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

        // GET: api/buildings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Building>>> GetBuildings()
        {
            // Використовуємо Include, щоб БД одразу віддала і будівлі, і їхні приміщення
            var buildings = await _context.Buildings
                                          .Include(b => b.Premises) 
                                          .ToListAsync();
            
            foreach (var b in buildings)
            {
                if (b.Location != null)
                {
                    b.Lat = b.Location.Y;
                    b.Lng = b.Location.X;
                }
            }
            return buildings;
        }

        // POST: api/buildings
        [HttpPost]
        public async Task<ActionResult<Building>> CreateBuilding(Building building)
        {
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            building.Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(building.Lng, building.Lat));

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBuildings), new { id = building.Id }, building);
        }

        // PUT: api/buildings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, Building building)
        {
            if (id != building.Id) return BadRequest("ID не співпадає.");

            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            building.Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(building.Lng, building.Lat));

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

        // DELETE: api/buildings/5
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