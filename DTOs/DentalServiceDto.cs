using api.Models;

namespace api.DTOs;

public class DentalServiceDto
{
    public string ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? ServiceDescription { get; set; }
    public decimal? Price { get; set; }
    public string? Unit { get; set; }
    public Status? Status { get; set; }
    public string? Benefit { get; set; }
    public IFormFile? FormFile { get; set; }
}