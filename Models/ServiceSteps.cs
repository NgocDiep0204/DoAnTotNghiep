using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class ServiceSteps
{
    [Key] public string Id { get; set; }
    public string? ServiceId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    [ForeignKey("ServiceId")] public DentalServices? DentalServices { get; set; }
}