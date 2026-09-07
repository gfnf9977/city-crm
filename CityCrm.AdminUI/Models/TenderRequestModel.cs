using System.ComponentModel.DataAnnotations;

namespace CityCrm.AdminUI.Models
{
    public class TenderRequestModel
    {
        public int Id { get; set; }
        public int PremiseId { get; set; }
        public Premise? Premise { get; set; }

        [Required(ErrorMessage = "Введіть назву компанії або ПІБ ФОП")]
        public string ApplicantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть ЄДРПОУ або ІПН")]
        public string Edrpou { get; set; } = string.Empty;

        [Required(ErrorMessage = "Контактна інформація обов'язкова")]
        public string ContactInfo { get; set; } = string.Empty;

        public string? Message { get; set; }

        [Required]
        public string RequestType { get; set; } = "Оренда";

        public string Status { get; set; } = "New";
        public string? AdminResponse { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}