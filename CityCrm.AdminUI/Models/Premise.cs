namespace CityCrm.AdminUI.Models
{
    public class Premise
    {
        public int Id { get; set; }
        public int BuildingId { get; set; }
        
        public string PremiseNumber { get; set; } = string.Empty; 
        public int? Entrance { get; set; } 
        public int? Floor { get; set; }
        public double Area { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Ownership { get; set; } = string.Empty;
        public string? OwnerName { get; set; } 
        public DateTime? RegistrationDate { get; set; } 
        public DateTime? RentEndDate { get; set; }
    }
}