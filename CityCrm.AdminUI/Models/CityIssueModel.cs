using System.ComponentModel.DataAnnotations;

namespace CityCrm.AdminUI.Models
{
    public class CityIssueModel
    {
        public int Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? AddressStr { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опишіть проблему")]
        public string Description { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
        public string Status { get; set; } = "New";
        public string? AdminResponse { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public int? UserRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}