using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CityCrm.Api.Entities
{
    public class Premise
    {
        public int Id { get; set; }
        
        public int BuildingId { get; set; }
        
        [JsonIgnore] 
        public Building? Building { get; set; }

        public string PremiseNumber { get; set; } = string.Empty; 
        public int? Entrance { get; set; } 
        public int? Floor { get; set; }
        
        public double Area { get; set; }
        public string Type { get; set; } = string.Empty; 
        public bool IsInclusive { get; set; } = false;
        public string Status { get; set; } = string.Empty; 
        public string Ownership { get; set; } = string.Empty; 
        public string? OwnerName { get; set; } 
        public string? Notes { get; set; }
        public string? BusinessCategory { get; set; } 
        public string? BusinessName { get; set; }     
        public string? WorkingHours { get; set; }     
        public string? BusinessDescription { get; set; } 
        public bool IsPublicVisible { get; set; } = false;
        
        public DateTime? RegistrationDate { get; set; } 
        
        public DateTime? RentEndDate { get; set; }
    }
}