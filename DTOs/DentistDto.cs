using api.Models;

namespace api.DTOs;

public class DentistDto
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public int? Years { get; set; }
    public string? Education { get; set; }
    public string Certificate { get; set; }
    public string? Introduce { get; set; }
    public string? Speacialty { get; set; }
    public string? Postgraduates { get; set; }
    public decimal? Price { get; set; }
    public Status? Status { get; set; }
}