using Microsoft.EntityFrameworkCore;
using SunucuBakimKontrol.Models;

namespace SunucuBakimKontrol.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<MaintenancePoint> MaintenancePoints { get; set; }
        
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            /*
            
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Server>().ToTable("servers");
            modelBuilder.Entity<MaintenancePoint>().ToTable("maintenancepoints");
            
            modelBuilder.Entity<MaintenanceLog>().ToTable("maintenancelogs");

            
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Server>().HasKey(s => s.ServerId);
            modelBuilder.Entity<MaintenancePoint>().HasKey(mp => mp.MaintenancePointId);
            
            modelBuilder.Entity<MaintenanceLog>().HasKey(ml => ml.LogId);

            İlişkileri yapılandırma bölümü
            modelBuilder.Entity<User>()
                .HasMany(u => u.Servers)
                .WithOne(s => s.Responsible)
                .HasForeignKey(s => s.ResponsibleId);

            modelBuilder.Entity<Server>()
                .HasMany(s => s.MaintenancePoints)
                .WithOne(mp => mp.Server)
                .HasForeignKey(mp => mp.ServerId);

            modelBuilder.Entity<Server>()
                .HasMany(s => s.MaintenanceLogs)
                .WithOne(ml => ml.Server)
                .HasForeignKey(ml => ml.ServerId);

           
            modelBuilder.Entity<MaintenancePoint>()
                .HasMany(mp => mp.MaintenanceLogs)
                .WithOne(ml => ml.MaintenancePoint)
                .HasForeignKey(ml => ml.MaintenancePointId); */
        }
    }
}
