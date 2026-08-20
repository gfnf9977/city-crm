using System.ComponentModel.DataAnnotations;

namespace CityCrm.AdminUI.Models
{
    public class Premise
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        
        [Required(ErrorMessage = "Номер приміщення є обов'язковим")]
        public string PremiseNumber { get; set; } = string.Empty; 
        
        [Range(1, 50, ErrorMessage = "Під'їзд має бути від 1 до 50")]
        public int? Entrance { get; set; } 
        
        [Range(-5, 50, ErrorMessage = "Поверх має бути від -5 до 50")]
        public int? Floor { get; set; }
        
        [Range(0.1, 10000, ErrorMessage = "Площа має бути більшою за 0")]
        public double Area { get; set; }
        
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Ownership { get; set; } = string.Empty;
        public string? OwnerName { get; set; } 
        public DateTime? RegistrationDate { get; set; } 
        public DateTime? RentEndDate { get; set; }
    }
}