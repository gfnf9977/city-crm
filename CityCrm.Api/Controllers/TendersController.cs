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
            request.TrackingNumber = $"TND-{DateTime.UtcNow:yyMM}-{new Random().Next(1000, 9999)}";

            _context.TenderRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { id = request.Id, trackingNumber = request.TrackingNumber });
        }

        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult> GetByTrackingNumber(string trackingNumber)
        {
            var req = await _context.TenderRequests
                .Where(t => t.TrackingNumber == trackingNumber)
                .Select(t => new {
                    id = t.Id,
                    premiseId = t.PremiseId,
                    applicantName = t.ApplicantName,
                    edrpou = t.Edrpou,
                    contactInfo = t.ContactInfo,
                    message = t.Message,
                    status = t.Status,
                    adminResponse = t.AdminResponse,
                    trackingNumber = t.TrackingNumber,
                    createdAt = t.CreatedAt,
                    premise = t.Premise == null ? null : new {
                        id = t.Premise.Id,
                        premiseNumber = t.Premise.PremiseNumber,
                        area = t.Premise.Area,
                        type = t.Premise.Type,
                        building = t.Premise.Building == null ? null : new {
                            id = t.Premise.Building.Id,
                            buildingNumber = t.Premise.Building.BuildingNumber,
                            buildingLetter = t.Premise.Building.BuildingLetter,
                            buildingBlock = t.Premise.Building.BuildingBlock,
                            buildingType = t.Premise.Building.BuildingType,
                            street = t.Premise.Building.Street == null ? null : new {
                                id = t.Premise.Building.Street.Id,
                                name = t.Premise.Building.Street.Name,
                                streetType = t.Premise.Building.Street.StreetType
                            }
                        }
                    }
                })
                .FirstOrDefaultAsync();

            if (req == null) return NotFound(new { message = "Заявку не знайдено" });
            return Ok(req);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpGet]
        public async Task<ActionResult> GetAllRequests()
        {
            var reqs = await _context.TenderRequests
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new {
                    id = t.Id,
                    premiseId = t.PremiseId,
                    applicantName = t.ApplicantName,
                    edrpou = t.Edrpou,
                    contactInfo = t.ContactInfo,
                    message = t.Message,
                    status = t.Status,
                    adminResponse = t.AdminResponse,
                    trackingNumber = t.TrackingNumber,
                    createdAt = t.CreatedAt,
                    premise = t.Premise == null ? null : new {
                        id = t.Premise.Id,
                        premiseNumber = t.Premise.PremiseNumber,
                        area = t.Premise.Area,
                        type = t.Premise.Type,
                        building = t.Premise.Building == null ? null : new {
                            id = t.Premise.Building.Id,
                            buildingNumber = t.Premise.Building.BuildingNumber,
                            buildingLetter = t.Premise.Building.BuildingLetter,
                            buildingBlock = t.Premise.Building.BuildingBlock,
                            buildingType = t.Premise.Building.BuildingType,
                            street = t.Premise.Building.Street == null ? null : new {
                                id = t.Premise.Building.Street.Id,
                                name = t.Premise.Building.Street.Name,
                                streetType = t.Premise.Building.Street.StreetType
                            }
                        }
                    }
                })
                .ToListAsync();

            return Ok(reqs);
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTenderStatusDto dto)
        {
            var req = await _context.TenderRequests
                .Include(t => t.Premise)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (req == null) return NotFound();

            req.Status = dto.Status;
            req.AdminResponse = dto.AdminResponse;

            if (req.Premise != null)
            {
                if (dto.Status == "Approved" && !string.IsNullOrWhiteSpace(dto.ProzorroLink))
                {
                    req.Premise.ProzorroLink = dto.ProzorroLink;
                }
                else if (dto.Status == "Completed")
                {
                    req.Premise.ProzorroLink = null;
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class UpdateTenderStatusDto 
    { 
        public string Status { get; set; } = string.Empty; 
        public string? AdminResponse { get; set; } 
        public string? ProzorroLink { get; set; }
        public string? FinalPremiseStatus { get; set; }
    }
}