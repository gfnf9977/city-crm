namespace CityCrm.AdminUI.Models
{
    public class Building
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string BuildingType { get; set; } = string.Empty; 
        public string Condition { get; set; } = "В експлуатації";
        
        public double Lat { get; set; }
        public double Lng { get; set; }

        public List<Premise> Premises { get; set; } = new();
    }
}