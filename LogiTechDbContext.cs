
using Microsoft.EntityFrameworkCore;
using LogiTech.Models;           

namespace LogiTech.Data
{
    public class LogiTechDbContext : DbContext
    {
        public LogiTechDbContext(DbContextOptions<LogiTechDbContext> options)
            : base(options) { }

        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<DeliveryStop> DeliveryStops { get; set; }
        public DbSet<LogiTech.Models.Route> Routes { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Prevent cascade delete issues
            modelBuilder.Entity<DeliveryStop>()
                .HasOne(d => d.Route)
                .WithMany(r => r.Stops)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.SetNull);
        }


    }

}