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
        public async Task<ActionResult> CreateRequest(BusinessRequest request)
        {
            request.Status = "Pending";
            request.CreatedAt = DateTime.UtcNow;
            
            _context.BusinessRequests.Add(request);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "GrandAdmin, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessRequest>>> GetPendingRequests()
        {
            return await _context.BusinessRequests
                .Include(r => r.Street)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.CreatedAt)
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
}