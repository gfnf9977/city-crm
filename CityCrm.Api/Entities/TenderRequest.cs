using System.ComponentModel.DataAnnotations;

namespace CityCrm.Api.Entities
{
    public class TenderRequest
    {
        public int Id { get; set; }

        [Required]
        public int PremiseId { get; set; }
        public Premise? Premise { get; set; }

        [Required]
        public string ApplicantName { get; set; } = string.Empty; 
        [Required]
        public string Edrpou { get; set; } = string.Empty;
        [Required]
        public string ContactInfo { get; set; } = string.Empty;
        public string? Message { get; set; } 

        [Required]
        public string RequestType { get; set; } = "Оренда";

        public string Status { get; set; } = "New"; 
        public string? AdminResponse { get; set; }

        public string TrackingNumber { get; set; } = string.Empty; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}