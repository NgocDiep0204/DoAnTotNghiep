using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Services> Services { get; set; }
        public DbSet<AppointmentDetails> AppointmentDetails { get; set; }
        public DbSet<Dentists> Dentists { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<RevokedToken> RevokedTokens {get;set;}
        public DbSet<OtpStorage> OtpStorages {get;set;}
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SeedRoles(modelBuilder);
            
            modelBuilder.Entity<Dentists>()
                .HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<Dentists>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OtpStorage>(entity =>
            {
                entity.HasOne(o => o.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)   
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(o => o.UserId);
            });

            modelBuilder.Entity<AppointmentDetails>()
                .HasKey(rd => new { rd.AppointmentId, rd.ServiceId });

            modelBuilder.Entity<AppointmentDetails>()
                .HasOne(rd => rd.Appointment)
                .WithMany(r => r.AppointmentDetails)
                .HasForeignKey(rd => rd.AppointmentId);
            
            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.Customers)
                .WithMany(u => u.CustomerReviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Quan hệ 1-N: Dentists -> Reviews
            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.Dentists)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DentistId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Quan hệ 1-N: Dentists -> Appointments
            modelBuilder.Entity<Appointments>()
                .HasOne(a => a.Dentists)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DentistId)
                .OnDelete(DeleteBehavior.Cascade);
        
        }
        private void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
                new IdentityRole() { Name = "Dentist", ConcurrencyStamp = "3", NormalizedName = "Dentist" }
            );
        }
        
    }
    
}