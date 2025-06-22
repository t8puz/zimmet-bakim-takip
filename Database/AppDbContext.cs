using System;
using System.IO;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zimmet_Bakim_Takip.Models;

namespace Zimmet_Bakim_Takip.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Device> Devices { get; set; }
        public DbSet<Personnel> Personnel { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Bakim> Bakimlar { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Debug.WriteLine("AppDbContext constructor çağrıldı");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                Debug.WriteLine("OnConfiguring çağrıldı ve henüz yapılandırılmamış");
                string dbPath = AppDbContextFactory.GetDbPath();
                Debug.WriteLine($"Veritabanı yolu: {dbPath}");
                
                // Log seviyesini ayarlama
                optionsBuilder
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(message => Debug.WriteLine(message), Microsoft.Extensions.Logging.LogLevel.Information);
                    
                // SQLite bağlantısı
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                Debug.WriteLine("SQLite bağlantısı yapılandırıldı");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Debug.WriteLine("OnModelCreating çağrıldı");
            
            // Device entity konfigürasyonu
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Devices");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.HasIndex(e => e.SerialNumber).IsUnique();
            });
            
            // Personnel entity konfigürasyonu
            modelBuilder.Entity<Personnel>(entity =>
            {
                entity.ToTable("Personnel");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
            });
            
            // Assignment entity konfigürasyonu
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.ToTable("Assignments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssignmentDate).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // İlişkiler
                entity.HasOne(d => d.Device)
                      .WithMany(p => p.Assignments)
                      .HasForeignKey(d => d.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(d => d.Personnel)
                      .WithMany(p => p.Assignments)
                      .HasForeignKey(d => d.PersonnelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // MaintenanceRecord entity konfigürasyonu
            modelBuilder.Entity<MaintenanceRecord>(entity =>
            {
                entity.ToTable("MaintenanceRecords");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaintenanceDate).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TechnicianName).HasMaxLength(100);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                
                // İlişkiler
                entity.HasOne(d => d.Device)
                      .WithMany(p => p.MaintenanceRecords)
                      .HasForeignKey(d => d.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Maintenance entity konfigürasyonu
            modelBuilder.Entity<Maintenance>(entity =>
            {
                entity.ToTable("Maintenances");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaintenanceDate).IsRequired();
                entity.Property(e => e.MaintenanceType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.Technician).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Company).HasMaxLength(100);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                
                // İlişkiler
                entity.HasOne(d => d.Device)
                      .WithMany()
                      .HasForeignKey(d => d.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Bakım ilişkileri
            modelBuilder.Entity<Bakim>()
                .HasOne(b => b.Cihaz)
                .WithMany(c => c.Bakimlar)
                .HasForeignKey(b => b.CihazId);
            
            // User entity konfigürasyonu
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Username).IsUnique();
            });
            
            // Attachment entity konfigürasyonu
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("Attachments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.RelatedEntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RelatedEntityId).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UploadDate).IsRequired();
                entity.Property(e => e.UploadedBy).HasMaxLength(100);
                
                // İlişkiler için bir indeks oluştur
                entity.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId });
            });
            
            base.OnModelCreating(modelBuilder);
            Debug.WriteLine("Model oluşturma tamamlandı");
        }
    }
} 