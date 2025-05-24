using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models;

public class Message
{
    [Key] public int Id { get; set; }

    public string Content { get; set; }

    public string Timestamp { get; set; }

    public string FromUserId { get; set; }

    public string ToUserId { get; set; }

    public virtual Room ToRoom { get; set; }
    public int Stick { get; set; }
}