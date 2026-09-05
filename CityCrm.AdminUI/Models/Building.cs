using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace CityCrm.AdminUI.Models
{
    public class Building
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Оберіть вулицю з довідника")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть вулицю з довідника")]
        public int StreetId { get; set; }
        
        public Street? Street { get; set; }

        [Required(ErrorMessage = "Номер об'єкту є обов'язковим")]
        [Range(1, 9999, ErrorMessage = "Некоректний номер будинку")]
        public int BuildingNumber { get; set; }
        
        public string? BuildingLetter { get; set; }
        public string? BuildingBlock { get; set; }
        public string? Notes { get; set; }
        public string? CoopNumber { get; set; }
        public bool HasShelter { get; set; } = false;
        [Required]
        public string BuildingType { get; set; } = string.Empty; 
        
        [Required]
        public string Condition { get; set; } = "В експлуатації";
        
        [Range(-90.0, 90.0, ErrorMessage = "Некоректна широта")]
        public double Lat { get; set; }
        
        [Range(-180.0, 180.0, ErrorMessage = "Некоректна довгота")]
        public double Lng { get; set; }
        
        public string? GeoJson { get; set; }

        public List<Premise> Premises { get; set; } = new();

        [JsonIgnore] 
        public string FullAddress
        {
            get
            {
                var stType = Street?.StreetType ?? "";
                var stName = Street?.Name ?? "";

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