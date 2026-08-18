using CityCrm.Api.Data;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!context.Buildings.Any())
    {
        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        
        var testBuilding = new CityCrm.Api.Entities.Building
        {
            Address = "Чернігів, проспект Миру, 15",
            BuildingType = "Багатоповерхівка",
            Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(31.2953, 51.4938)),
            
            Premises = new List<CityCrm.Api.Entities.Premise>
            {
                new CityCrm.Api.Entities.Premise 
                { 
                    PremiseNumber = "Офіс 1", 
                    Area = 120.5, 
                    Type = "Комерційна", 
                    Status = "Вільне", 
                    Ownership = "Комунальна" 
                }
            }
        };
        
        context.Buildings.Add(testBuilding);
        context.SaveChanges();
    }
}

app.Run();