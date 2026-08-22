using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace CityCrm.AdminUI.Models
{
    public class Building
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Тип вулиці є обов'язковим")]
        public string StreetType { get; set; } = "вул."; 
        
        [Required(ErrorMessage = "Назва вулиці є обов'язковою")]
        public string StreetName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Номер об'єкту є обов'язковим")]
        [Range(1, 9999, ErrorMessage = "Некоректний номер будинку")]
        public int BuildingNumber { get; set; }
        
        public string? BuildingLetter { get; set; }
        public string? Notes { get; set; }
        
        public string? CoopNumber { get; set; }

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
        public string FullAddress => BuildingType == "Гаражний кооператив" && !string.IsNullOrWhiteSpace(CoopNumber) 
            ? $"АК №{CoopNumber}, {StreetType} {StreetName}, {BuildingNumber}{BuildingLetter}" 
            : $"{StreetType} {StreetName}, {BuildingNumber}{BuildingLetter}";
    }
}