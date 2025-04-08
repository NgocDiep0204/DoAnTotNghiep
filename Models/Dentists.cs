using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Dentists
{
    public required string Id { get; set; }
    public string? UserId { get; set; }
    public string? Speacialty { get; set; }

    [ForeignKey("UserId")] public ApplicationUser? User { get; set; }

    public ICollection<Appointments>? Appointments { get; set; }
    public ICollection<Reviews>? Reviews { get; set; }
}