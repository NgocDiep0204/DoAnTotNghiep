using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Prescriptions
{
    [Key] public required string PrescriptionId { get; set; }

    [ForeignKey("AppointmentId")] public string? AppointmentId { get; set; }

    public string? MedicineName { get; set; }
    public string? Dosage { get; set; }
    public string? Instruction { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public Appointments? Appointments { get; set; }
}