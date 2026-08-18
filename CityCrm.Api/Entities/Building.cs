using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace CityCrm.Api.Entities
{
    public class Building
    {
        public int Id { get; set; }
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public string BuildingType { get; set; } = string.Empty; 
        public string Condition { get; set; } = "В експлуатації";

        [JsonIgnore]
        public Point? Location { get; set; }

        [NotMapped] 
        public double Lat { get; set; }

        [NotMapped]
        public double Lng { get; set; }

        public List<Premise> Premises { get; set; } = new();
    }
}