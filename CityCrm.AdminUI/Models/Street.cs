namespace CityCrm.AdminUI.Models
{
    public class Street
    {
        public int Id { get; set; }
        public string StreetType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? OldNames { get; set; }
        
        public string DisplayName => string.IsNullOrEmpty(OldNames) 
            ? $"{StreetType} {Name}" 
            : $"{StreetType} {Name} (колиш. {OldNames})";
    }
}