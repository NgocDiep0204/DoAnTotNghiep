using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class AppointmentDetails
{
    [Key] public required string AppointmentId { get; set; }

    public string? ServiceId { get; set; }
    public int? Quantity { get; set; }

    [ForeignKey("ServiceId")] public DentalServices? Services { get; set; }

    [ForeignKey("AppointmentId")] public Appointments? Appointment { get; set; }
}