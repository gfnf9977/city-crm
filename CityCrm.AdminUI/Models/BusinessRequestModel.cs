using System.ComponentModel.DataAnnotations;

namespace CityCrm.AdminUI.Models 
{
    public class BusinessRequestModel 
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Street? Street { get; set; }

        [Required(ErrorMessage = "Введіть ЄДРПОУ або ІПН")]
        public string Edrpou { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть юридичну назву")]
        public string LegalName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть телефон або email")]
        public string ContactInfo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть сферу діяльності")]
        public string BusinessCategory { get; set; } = string.Empty;

        public bool IsNetwork { get; set; }
        public string? NetworkName { get; set; }

        [Required(ErrorMessage = "Введіть назву закладу")]
        public string LocalName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Оберіть вулицю зі списку")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть вулицю зі списку")]
        public int StreetId { get; set; }

        [Required(ErrorMessage = "Номер будинку обов'язковий")]
        [Range(1, 9999, ErrorMessage = "Некоректний номер")]
        public int BuildingNumber { get; set; }

        public string? BuildingLetter { get; set; }
        public string? BuildingBlock { get; set; }
        public string? PremiseNumber { get; set; }

        public string? WorkingHours { get; set; }
        public bool IsInclusive { get; set; }

        public string? ReferenceLink { get; set; }
    }
}