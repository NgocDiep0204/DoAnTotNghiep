using api.Models;
using Chat.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointments> Appointments { get; set; }
    public DbSet<DentalServices> DentalServices { get; set; }
    public DbSet<AppointmentDetails> AppointmentDetails { get; set; }
    public DbSet<Dentists> Dentists { get; set; }
    public DbSet<Payments> Payments { get; set; }
    public DbSet<Reviews> Reviews { get; set; }
    public DbSet<RevokedToken> RevokedTokens { get; set; }
    public DbSet<OtpStorage> OtpStorages { get; set; }

    public DbSet<Message> Messages { get; set; }
    public DbSet<UserRoom> UserRooms { get; set; }
    public DbSet<ServiceSteps> ServiceSteps { get; set; }
    public DbSet<Posts> Posts { get; set; }
    public DbSet<Comments> Comments { get; set; }
    public DbSet<ImagePost> ImagePosts { get; set; }

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
        
        // Chat
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.MessagesSent)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.MessagesReceived)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // UserRoom
        modelBuilder.Entity<UserRoom>()
            .HasMany(r => r.Users)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserRoomMembers")); 

        // 🔹 Quan hệ 1-N: Dentists -> Appointments
        modelBuilder.Entity<Appointments>()
            .HasOne(a => a.Dentists)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DentistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comments>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict); // tránh xóa đệ quy gây lỗi
    }

    private void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
            new IdentityRole { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
            new IdentityRole { Name = "Dentist", ConcurrencyStamp = "3", NormalizedName = "Dentist" }
        );
    }
}