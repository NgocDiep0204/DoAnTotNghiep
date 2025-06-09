using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.Models;

namespace api.Appointment.Model;

public class AppointmentSchedule
{
    [Key]
    public required string ID { get; set; }

    [Required]
    public required string DentistId { get; set; }

    public DateOnly? Date { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public bool? IsFree { get; set; } = true;

    public bool? IsDayOff { get; set; } = false;

    [ForeignKey("DentistId")]
    public Dentists Dentist { get; set; } = null!;
}