using System.ComponentModel.DataAnnotations;

namespace CityCrm.Api.Entities
{
    public class Mural
    {
        public int Id { get; set; }
        
        [Required] 
        public string Title { get; set; } = string.Empty;
        
        public string? Artist { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public string PhotoUrl { get; set; } = string.Empty;
        
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Address { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Гарний стан"; 
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}