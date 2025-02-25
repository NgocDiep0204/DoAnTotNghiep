using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class AppointmentDetails
{
    [Key]
    public required string AppointmentId { get; set; }
    public required string ServiceId { get; set; }
    public required int Quantity { get; set; }
    [ForeignKey("ServiceId")]
    public Services? Services { get; set; }

    [ForeignKey("AppointmentId")] public Appointments? Appointment { get; set; }
}