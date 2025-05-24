using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models;

public class Room
{
    [Key] public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Message> Messages { get; set; }

    public virtual ICollection<UserRoom> UserRoom { get; set; }
}