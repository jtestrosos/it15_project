using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using production_system.Models;

namespace production_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly production_system.Services.IEncryptionService? _encryptionService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, production_system.Services.IEncryptionService? encryptionService = null) : base(options)
        {
            _encryptionService = encryptionService;
        }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<ProductionPlan> ProductionPlans { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<BillOfMaterial> BillOfMaterials { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Cost> Costs { get; set; }
        public DbSet<EnvironmentalMonitor> EnvironmentalMonitors { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ArchivedUser> ArchivedUsers { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (_encryptionService != null)
            {
                var converter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
                    v => _encryptionService.Encrypt(v) ?? string.Empty,
                    v => _encryptionService.Decrypt(v) ?? string.Empty
                );

                builder.Entity<Facility>()
                    .Property(f => f.APIKeyOpenWeather)
                    .HasConversion(converter);

                builder.Entity<Facility>()
                    .Property(f => f.APIKeyExchangeRate)
                    .HasConversion(converter);
            }

            // Prevent cascade delete loops
            builder.Entity<ProductionPlan>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkOrder>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryTransaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ActivityLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

