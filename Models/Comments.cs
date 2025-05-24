using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Comments
{
    public required string Id { get; set; }
    public string? PostId { get; set; }
    public string? UserId { get; set; }
    public string? ParentCommentId { get; set; }
    public string? Content { get; set; }
    public string? Image { get; set; }
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")] public ApplicationUser User { get; set; }

    [ForeignKey("PostId")] public Posts Post { get; set; }

    public Comments? ParentComment { get; set; }
    public ICollection<Comments> Replies { get; set; } = new List<Comments>();
}