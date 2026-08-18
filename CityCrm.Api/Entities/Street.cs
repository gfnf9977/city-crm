namespace CityCrm.Api.Entities
{
    public class Street
    {
        public int Id { get; set; }
        
        public string StreetType { get; set; } = string.Empty;
        
        public string Name { get; set; } = string.Empty;
        
        public string? OldNames { get; set; } 
    }
}