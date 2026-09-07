using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TendersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TendersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateTenderRequest(TenderRequest request)
        {
            var premise = await _context.Premises.FindAsync(request.PremiseId);
            if (premise == null) return NotFound("Приміщення не знайдено.");
            if (premise.Ownership != "Комунальна" || premise.Status != "Вільне" || premise.Type != "Комерційна")
            {
                return BadRequest("Це приміщення недоступне для комерційної оренди.");
            }

            request.Status = "New";
            request.CreatedAt = DateTime.UtcNow;
            
            var random = new Random();
            request.TrackingNumber = $"TND-{DateTime.UtcNow:yyMM}-{random.Next(1000, 9999)}";

            _context.TenderRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { id = request.Id, trackingNumber = request.TrackingNumber });
        }

        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult> GetByTrackingNumber(string trackingNumber)
        {
            var req = await _context.TenderRequests
                .Include(t => t.Premise)
                .ThenInclude(p => p.Building)
                .ThenInclude(b => b.Street)
                .FirstOrDefaultAsync(t => t.TrackingNumber == trackingNumber);

            if (req == null) return NotFound(new { message = "Заявку не знайдено" });

            return Ok(req);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TenderRequest>>> GetAllRequests()
        {
            return await _context.TenderRequests
                .Include(t => t.Premise)
                .ThenInclude(p => p.Building)
                .ThenInclude(b => b.Street)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTenderStatusDto dto)
        {
            var req = await _context.TenderRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = dto.Status;
            req.AdminResponse = dto.AdminResponse;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class UpdateTenderStatusDto 
    { 
        public string Status { get; set; } = string.Empty; 
        public string? AdminResponse { get; set; } 
    }
}