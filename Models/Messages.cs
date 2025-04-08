using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class Messages
{
    [Key] public required string MessageId { get; set; }

    public string? SenderId { get; set; }
    public string? MessageText { get; set; }
    public MessageType? MessageType { get; set; }
    public MessageStatus? MessageStatus { get; set; }
    public DateTime? SentAt { get; set; }
    public bool? IsSystem { get; set; }
}

public enum MessageType
{
    Text,
    Image,
    File
}

public enum MessageStatus
{
    Sent,
    Delivered,
    Read
}