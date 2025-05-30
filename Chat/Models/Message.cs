using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.Models;

namespace Chat.Web.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string SenderId { get; set; } = default!;
    [ForeignKey("SenderId")]
    public virtual ApplicationUser Sender { get; set; } = default!;

    [Required]
    public string? ReceiverId { get; set; } 
    [ForeignKey("ReceiverId")]
    public virtual ApplicationUser? Receiver { get; set; } 
    
    public Guid? RoomId { get; set; }
    public virtual UserRoom? Room { get; set; }

}