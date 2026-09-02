using CityCrm.Api.Data;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<CityCrm.Api.Services.OsmService>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    context.Database.EnsureCreated();
    
    if (!context.Streets.Any())
    {
        var seedFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "streets.json");
        if (File.Exists(seedFilePath))
        {
            var jsonData = File.ReadAllText(seedFilePath);
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var streets = System.Text.Json.JsonSerializer.Deserialize<List<CityCrm.Api.Entities.Street>>(jsonData, options);

            if (streets != null && streets.Any())
            {
                context.Streets.AddRange(streets);
                context.SaveChanges();
            }
        }
    }
    
    if (!context.Buildings.Any() && context.Streets.Any())
    {
        var firstStreet = context.Streets.First(s => s.Name == "Миру");
        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        
        var testBuilding = new CityCrm.Api.Entities.Building
        {
            StreetId = firstStreet.Id,
            BuildingNumber = 15,
            BuildingType = "Багатоповерхівка",
            Condition = "В експлуатації", 
            Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(31.2953, 51.4938)),
            
            Premises = new List<CityCrm.Api.Entities.Premise>
            {
                new CityCrm.Api.Entities.Premise 
                { 
                    PremiseNumber = "Офіс 1", Area = 120.5, Type = "Комерційна", 
                    Status = "Вільне", Ownership = "Комунальна" 
                }
            }
        };
        
        context.Buildings.Add(testBuilding);
        context.SaveChanges();
    }
}

app.Run();