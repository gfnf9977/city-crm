using System.ComponentModel.DataAnnotations;

namespace CityCrm.Api.Entities
{
    public class BusinessRequest
    {
        public int Id { get; set; }

        public string Edrpou { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;

        public string BusinessCategory { get; set; } = string.Empty;
        public bool IsNetwork { get; set; } = false;
        public string? NetworkName { get; set; } 
        public string? LocalName { get; set; } 
        public string? Description { get; set; }

        public int StreetId { get; set; }
        public Street? Street { get; set; }
        public int BuildingNumber { get; set; }
        public string? BuildingLetter { get; set; }
        public string? BuildingBlock { get; set; }
        public string? PremiseNumber { get; set; }

        public string? WorkingHours { get; set; } 
        public bool IsInclusive { get; set; }
        
        public string? ReferenceLink { get; set; } 

        public string Status { get; set; } = "Pending"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}