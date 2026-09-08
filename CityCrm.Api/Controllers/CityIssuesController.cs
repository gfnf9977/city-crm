using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityIssuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CityIssuesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateIssue(CityIssue issue)
        {
            issue.Status = "New";
            issue.CreatedAt = DateTime.UtcNow;
            
            var random = new Random();
            issue.TrackingNumber = $"INC-{DateTime.UtcNow:yyMM}-{random.Next(1000, 9999)}";

            _context.CityIssues.Add(issue);
            await _context.SaveChangesAsync();

            return Ok(new { id = issue.Id, trackingNumber = issue.TrackingNumber });
        }

        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult> GetByTrackingNumber(string trackingNumber)
        {
            var issue = await _context.CityIssues.FirstOrDefaultAsync(i => i.TrackingNumber == trackingNumber);
            if (issue == null) return NotFound(new { message = "Інцидент не знайдено" });

            return Ok(issue);
        }

        [HttpPut("{id}/rate")]
        public async Task<IActionResult> RateIssue(int id, [FromBody] RateDto dto)
        {
            var issue = await _context.CityIssues.FindAsync(id);
            if (issue == null) return NotFound();

            if (issue.Status != "Resolved")
                return BadRequest("Оцінити можна лише вирішену проблему.");

            issue.UserRating = dto.Rating;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("map")]
        public async Task<ActionResult> GetIssuesForMap([FromQuery] string? bbox)
        {
            var query = _context.CityIssues.Where(i => i.Status != "Rejected").AsQueryable();

            if (!string.IsNullOrWhiteSpace(bbox))
            {
                var coords = bbox.Split(',');
                if (coords.Length == 4 &&
                    double.TryParse(coords[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double minLng) &&
                    double.TryParse(coords[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double minLat) &&
                    double.TryParse(coords[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double maxLng) &&
                    double.TryParse(coords[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double maxLat))
                {
                    query = query.Where(i => i.Lat >= minLat && i.Lat <= maxLat && i.Lng >= minLng && i.Lng <= maxLng);
                }
            }

            var issues = await query
                .Select(i => new {
                    i.Id, i.Lat, i.Lng, i.Category, i.Status, i.Description, i.CreatedAt
                })
                .ToListAsync();

            return Ok(issues);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityIssue>>> GetAllIssues()
        {
            return await _context.CityIssues
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var issue = await _context.CityIssues.FindAsync(id);
            if (issue == null) return NotFound();

            issue.Status = dto.Status;
            issue.AdminResponse = dto.AdminResponse;

            if (dto.Status == "Resolved")
                issue.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class RateDto { public int Rating { get; set; } }
    public class UpdateStatusDto { public string Status { get; set; } = string.Empty; public string? AdminResponse { get; set; } }
}