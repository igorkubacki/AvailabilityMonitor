using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AvailabilityMonitor.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<AvailabilityMonitor.Models.Product>? Product { get; set; }
        public DbSet<AvailabilityMonitor.Models.Config>? Config { get; set; }
        public DbSet<AvailabilityMonitor.Models.PriceChange>? PriceChange { get; set; }
        public DbSet<AvailabilityMonitor.Models.QuantityChange>? QuantityChange { get; set; }
    }
}