using System.ComponentModel.DataAnnotations.Schema;
using api.Appointment.Model;

namespace api.Models;

public class Dentists
{
    public required string Id { get; set; }
    public string? UserId { get; set; }
    public int? Years { get; set; }
    public string? Education { get; set; }
    public string? Certificate { get; set; }
    public string? Introduce { get; set; }
    public string? Speacialty { get; set; }
    public Status? Status { get; set; }
    public string? Postgraduates {get; set;}
    public decimal? Price { get; set; }

    [ForeignKey("UserId")] public ApplicationUser? User { get; set; }

    public ICollection<Appointments>? Appointments { get; set; }
    
    public ICollection<AppointmentSchedule> AppointmentSchedules { get; set; } = new List<AppointmentSchedule>();
}

public enum Status
{
    active,
    inactive
}