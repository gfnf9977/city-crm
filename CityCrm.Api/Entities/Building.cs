using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace CityCrm.Api.Entities
{
    public class Building
    {
        public int Id { get; set; }
        
        public int StreetId { get; set; }
        public Street? Street { get; set; }

        public int BuildingNumber { get; set; }
        public string? BuildingLetter { get; set; }
        public string? BuildingBlock { get; set; }
        public string? CoopNumber { get; set; }
        public string? Notes { get; set; }
        public bool HasShelter { get; set; } = false;
        public string BuildingType { get; set; } = string.Empty; 
        public string Condition { get; set; } = "В експлуатації";

        [JsonIgnore]
        public Geometry? Location { get; set; }

        [NotMapped] 
        public double Lat { get; set; }

        [NotMapped]
        public double Lng { get; set; }

        [NotMapped]
        public string? GeoJson { get; set; }

        public List<Premise> Premises { get; set; } = new();

        [NotMapped]
        public string FullAddress
        {
            get
            {
                var stType = Street?.StreetType ?? "";
                var stName = Street?.Name ?? "Невідома вулиця";

                var addr = BuildingType == "Гаражний кооператив" && !string.IsNullOrWhiteSpace(CoopNumber)
                    ? $"АК №{CoopNumber}, {stType} {stName}, {BuildingNumber}{BuildingLetter}"
                    : $"{stType} {stName}, {BuildingNumber}{BuildingLetter}";

                if (!string.IsNullOrWhiteSpace(BuildingBlock))
                {
                    addr += $", корп. {BuildingBlock}";
                }

                return addr.Trim(',', ' ');
            }
        }
    }
}