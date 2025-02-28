using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class DentalServices
{
    [Key] public string ServiceId { get; set; }

    public string ServiceName { get; set; }
    public string ServiceDescription { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<AppointmentDetails>? AppointmentDetails { get; set; }
}