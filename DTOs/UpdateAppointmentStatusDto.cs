using api.Models;

namespace api.DTOs;

public class UpdateAppointmentStatusDto
{
    public string? AppointmentId { get; set; }
    public string? DentistId { get; set; }
    public string? DentisNotes  { get; set; }
    public AppointmentStatus? Status { get; set; }   
}