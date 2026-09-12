using CityCrm.Api.Data;
using CityCrm.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MuralsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MuralsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mural>>> GetMurals()
        {
            return await _context.Murals.Where(m => m.Status != "Знищено").ToListAsync();
        }

        [HttpPost("optimize-route")]
        public async Task<ActionResult> OptimizeRoute([FromBody] RouteRequest request)
        {
            var murals = await _context.Murals
                .Where(m => request.UnvisitedMuralIds.Contains(m.Id) && m.Status != "Знищено")
                .ToListAsync();

            if (!murals.Any()) return Ok(new List<Mural>());

            var route = new List<Mural>();
            var currentLat = request.StartLat;
            var currentLng = request.StartLng;
            var unvisited = murals.ToList();

            while (unvisited.Any())
            {
                var nearest = unvisited
                    .OrderBy(m => GetDistance(currentLat, currentLng, m.Lat, m.Lng))
                    .First();
                
                route.Add(nearest);
                unvisited.Remove(nearest);
                
                currentLat = nearest.Lat;
                currentLng = nearest.Lng;
            }

            return Ok(route);
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // Радіус Землі в метрах
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                    
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; 
        }
    }

    public class RouteRequest
    {
        public double StartLat { get; set; }
        public double StartLng { get; set; }
        public List<int> UnvisitedMuralIds { get; set; } = new();
    }
}