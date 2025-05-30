using api.Models;

namespace api.DTOs;

public class PostDto
{
    public string? UserId { get; set; }
    public string? Content { get; set; }
    public Status? Status { get; set; }
}