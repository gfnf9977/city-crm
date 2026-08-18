using System.Text.Json.Serialization; 

namespace CityCrm.AdminUI.Models
{
    public class Building
    {
        public int Id { get; set; }
        
        public string StreetType { get; set; } = "вул."; 
        public string StreetName { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string? CoopNumber { get; set; }

        public string BuildingType { get; set; } = string.Empty; 
        public string Condition { get; set; } = "В експлуатації";
        
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? GeoJson { get; set; }

        public List<Premise> Premises { get; set; } = new();

        [JsonIgnore] 
        public string FullAddress => BuildingType == "Гаражний кооператив" && !string.IsNullOrWhiteSpace(CoopNumber) 
            ? $"АК №{CoopNumber}, {StreetType} {StreetName}, {BuildingNumber}" 
            : $"{StreetType} {StreetName}, {BuildingNumber}";
    }
}