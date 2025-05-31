using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Appointments
{
    [Key] public required string AppointmentId { get; set; }

    public string? CustomerId { get; set; }
    public string? DentistId { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public AppointmentStatus? Status { get; set; }
    public string? Notes { get; set; }
    public string? DentistNotes { get; set; }
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CustomerId")] public ApplicationUser? Customers { get; set; }

    [ForeignKey("DentistId")] public Dentists? Dentists { get; set; }

    public ICollection<AppointmentDetails>? AppointmentDetails { get; set; }
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Canceled
}

public static class AppointmentStatusExtensions
{
    public static string GetDescriptions(this AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Pending => "pending",
            AppointmentStatus.Confirmed => "confirmed",
            AppointmentStatus.Completed => "completed",
            AppointmentStatus.Canceled => "canceled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}