using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class DentalServices
{
    [Key] public string ServiceId { get; set; }

    public string? ServiceName { get; set; }
    public string? ServiceDescription { get; set; }
    public string? Benefit { get; set; }
    public string? ImgService { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Status? Status { get; set; }
    public ICollection<AppointmentDetails>? AppointmentDetails { get; set; }
    public ICollection<ServiceSteps>? ServiceSteps { get; set; }
}