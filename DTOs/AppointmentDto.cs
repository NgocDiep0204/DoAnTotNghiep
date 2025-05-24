namespace api.DTOs;

public class AppointmentDto
{
    public string? CustomerId { get; set; }
    public string? DentistId { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? Notes { get; set; }
}