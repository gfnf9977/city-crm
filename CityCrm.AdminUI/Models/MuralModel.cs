namespace CityCrm.AdminUI.Models
{
    public class MuralModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Artist { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}