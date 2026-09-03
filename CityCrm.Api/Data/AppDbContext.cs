using CityCrm.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityCrm.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Premise> Premises => Set<Premise>(); 
        public DbSet<Street> Streets { get; set; }
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<Premise>()
                .HasOne(p => p.Building)
                .WithMany(b => b.Premises)
                .HasForeignKey(p => p.BuildingId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}