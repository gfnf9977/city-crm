using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PremisesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PremisesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/premises
        [HttpPost]
        public async Task<ActionResult<Premise>> CreatePremise(Premise premise)
        {
            // Перевіряємо, чи існує будівля, до якої ми додаємо приміщення
            var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == premise.BuildingId);
            if (!buildingExists)
            {
                return BadRequest("Будівлю не знайдено.");
            }

            _context.Premises.Add(premise);
            await _context.SaveChangesAsync();

            return Ok(premise);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePremise(int id, Premise premise)
        {
            if (id != premise.Id) return BadRequest("ID не співпадає.");
            _context.Entry(premise).State = EntityState.Modified;
            
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Premises.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // DELETE: api/premises/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePremise(int id)
        {
            var premise = await _context.Premises.FindAsync(id);
            if (premise == null) return NotFound();

            _context.Premises.Remove(premise);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}