using System.ComponentModel.DataAnnotations;
using api.Models;

namespace Chat.Web.Models;

public class UserRoom
{
    [Key]
    public Guid Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}