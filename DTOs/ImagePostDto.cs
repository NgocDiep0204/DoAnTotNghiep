namespace api.DTOs;

public class ImagePostDto
{
    public string? PostId { get; set; }
    public IFormFile? File { get; set; }
}