using api.Models;

namespace api.DTOs;

public class ApplicationUserDto
{
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public IFormFile? FormFile { get; set; }
}