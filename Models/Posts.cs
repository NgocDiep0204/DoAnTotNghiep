using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Posts
{
    [Key] public required string PostId { get; set; }
    public string? UserId { get; set; }
    public string? Content { get; set; }
    public StatusPost? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    [ForeignKey("UserId")] public ApplicationUser? User { get; set; }
    public ICollection<ImagePost>? ImagePosts { get; set; }
}

public enum StatusPost
{
    Pending,
    Approved,
    Rejected,
}