namespace api.DTOs;

public class AppointmentDetailDto
{
    public string? AppointmentId { get; set; }

    public string? ServiceId { get; set; }
    public int? Quantity { get; set; }
}