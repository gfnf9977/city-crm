using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        [HttpGet("{id}")]
        public async Task<ActionResult<Premise>> GetPremise(int id)
        {
            var premise = await _context.Premises
                .Include(p => p.Building)
                .ThenInclude(b => b.Street)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (premise == null) return NotFound();
            return Ok(premise);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPost]
        public async Task<ActionResult<Premise>> CreatePremise(Premise premise)
        {
            var conflictError = ValidatePremiseStatusConflicts(premise);
            if (conflictError != null) return BadRequest(conflictError);

            var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == premise.BuildingId);
            if (!buildingExists)
            {
                return BadRequest("Будівлю не знайдено.");
            }

            _context.Premises.Add(premise);
            await _context.SaveChangesAsync();

            return Ok(premise);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePremise(int id, Premise premise)
        {
            if (id != premise.Id) return BadRequest("ID не співпадає.");

            var conflictError = ValidatePremiseStatusConflicts(premise);
            if (conflictError != null) return BadRequest(conflictError);

            _context.Entry(premise).State = EntityState.Modified;
            
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Premises.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePremise(int id)
        {
            var premise = await _context.Premises.FindAsync(id);
            if (premise == null) return NotFound();

            _context.Premises.Remove(premise);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}/prozorro")]
        public async Task<IActionResult> SetProzorroLink(int id, [FromBody] ProzorroDto dto)
        {
            var premise = await _context.Premises.FindAsync(id);
            if (premise == null) return NotFound();

            premise.ProzorroLink = dto.Link;
            premise.IsPublicVisible = true;

            var systemTender = new TenderRequest
            {
                PremiseId = premise.Id,
                ApplicantName = "Фонд комунального майна (Міськрада)",
                Edrpou = "ЄДРПОУ Міськради",
                ContactInfo = "Пряма публікація",
                Message = "Ініційовано містом",
                Status = "Approved",
                AdminResponse = $"Посилання на лот: {dto.Link}",
                TrackingNumber = $"TND-SYS-{DateTime.UtcNow:yyMM}-{new Random().Next(1000, 9999)}",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.TenderRequests.Add(systemTender);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        private string? ValidatePremiseStatusConflicts(Premise p)
        {
            if (p.Ownership == "Приватна" || p.Ownership == "Державна")
            {
                if (p.Status != "В експлуатації" && p.Status != "Аварійне")
                    return $"Недопустимий статус '{p.Status}' для форми власності '{p.Ownership}'.";
            }
            else if (p.Ownership == "Комунальна")
            {
                if (p.Status == "В експлуатації")
                    return "Для комунальної власності потрібен точний статус (Вільне, Орендоване, Службове тощо).";
                    
                if (p.Type == "Житлова" && p.Status == "Орендоване (Комерція)")
                    return "Житлове приміщення не може бути в комерційній оренді.";
                    
                if (p.Type == "Комерційна" && p.Status == "Орендоване (Соціальне)")
                    return "Комерційне приміщення не підходить для соціальної оренди.";
            }
            return null;
        }
    }

    public class ProzorroDto
    { 
        [Required(ErrorMessage = "Посилання є обов'язковим.")]
        [Url(ErrorMessage = "Це має бути коректне веб-посилання (починатися з http:// або https://)")]
        public string Link { get; set; } = string.Empty; 
    }
}