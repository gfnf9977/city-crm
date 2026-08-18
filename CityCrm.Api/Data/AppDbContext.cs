using CityCrm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Premise> Premises => Set<Premise>(); // Додали нову таблицю

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("postgis");

            // Явно вказуємо зв'язок (хоча EF Core зазвичай розуміє це сам)
            modelBuilder.Entity<Premise>()
                .HasOne(p => p.Building)
                .WithMany(b => b.Premises)
                .HasForeignKey(p => p.BuildingId)
                .OnDelete(DeleteBehavior.Cascade); // Якщо видалити будівлю, видаляться і всі її приміщення
        }
    }
}