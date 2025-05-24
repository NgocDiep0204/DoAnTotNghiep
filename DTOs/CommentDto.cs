namespace api.DTOs;

public class CommentDto
{
    public string? PostId { get; set; }
    public string? UserId { get; set; }
    public string? Content { get; set; }
    public IFormFile? File { get; set; }
    public string? ParentCommentId { get; set; }
}