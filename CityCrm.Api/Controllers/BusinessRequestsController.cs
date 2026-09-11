using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusinessRequestsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRequest([FromBody] BulkBusinessRequestDto bulkDto)
        {
            var trackingNumber = $"BIZ-{DateTime.UtcNow:yyMM}-{new Random().Next(1000, 9999)}";
            var requests = new List<BusinessRequest>();
            var now = DateTime.UtcNow;

            foreach (var branch in bulkDto.Branches)
            {
                requests.Add(new BusinessRequest
                {
                    Status = "Pending",
                    CreatedAt = now,
                    TrackingNumber = trackingNumber,
                    Edrpou = bulkDto.Edrpou,
                    LegalName = bulkDto.LegalName,
                    ContactInfo = bulkDto.ContactInfo,
                    BusinessCategory = bulkDto.BusinessCategory,
                    IsNetwork = bulkDto.IsNetwork,
                    NetworkName = bulkDto.NetworkName,
                    IsInclusive = bulkDto.IsInclusive,
                    ReferenceLink = bulkDto.ReferenceLink,
                    WorkingHours = bulkDto.WorkingHours,
                    
                    LocalName = string.IsNullOrWhiteSpace(branch.LocalName) ? null : branch.LocalName,
                    Description = string.IsNullOrWhiteSpace(branch.Description) ? bulkDto.GeneralDescription : branch.Description,
                    
                    StreetId = branch.StreetId,
                    BuildingNumber = branch.BuildingNumber,
                    BuildingLetter = branch.BuildingLetter,
                    BuildingBlock = branch.BuildingBlock,
                    PremiseNumber = branch.PremiseNumber
                });
            }
            
            _context.BusinessRequests.AddRange(requests);
            await _context.SaveChangesAsync();
            
            return Ok(new { trackingNumber = trackingNumber });
        }

        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult> GetRequestStatus(string trackingNumber)
        {
            var reqs = await _context.BusinessRequests
                .Include(b => b.Street)
                .Where(b => b.TrackingNumber == trackingNumber)
                .ToListAsync();
                
            if (!reqs.Any()) return NotFound(new { message = "Заявку не знайдено" });

            var first = reqs.First();
            var networkName = first.IsNetwork ? first.NetworkName : first.LocalName;

            var branches = reqs.Select(r => new {
                name = r.IsNetwork ? (string.IsNullOrWhiteSpace(r.LocalName) ? "Стандартне відділення" : r.LocalName) : r.LocalName,
                address = $"{r.Street?.StreetType} {r.Street?.Name}, {r.BuildingNumber}{r.BuildingLetter}",
                status = r.Status
            });

            return Ok(new 
            { 
                trackingNumber = trackingNumber,
                name = networkName,
                branches = branches
            });
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessRequest>>> GetAllRequests()
        {
            return await _context.BusinessRequests
                .Include(r => r.Street)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.BusinessRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Rejected";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var req = await _context.BusinessRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = "Approved";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class BulkBusinessRequestDto
    {
        public string Edrpou { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string BusinessCategory { get; set; } = string.Empty;
        public bool IsNetwork { get; set; }
        public string? NetworkName { get; set; }
        public string? GeneralDescription { get; set; }
        public bool IsInclusive { get; set; }
        public string? ReferenceLink { get; set; }
        public string? WorkingHours { get; set; }
        public List<BranchDto> Branches { get; set; } = new();
    }

    public class BranchDto
    {
        public string? LocalName { get; set; }
        public string? Description { get; set; }
        public int StreetId { get; set; }
        public int BuildingNumber { get; set; }
        public string? BuildingLetter { get; set; }
        public string? BuildingBlock { get; set; }
        public string? PremiseNumber { get; set; }
    }
}