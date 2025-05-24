using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Dentists
{
    public required string Id { get; set; }
    public string? UserId { get; set; }
    public int? Years { get; set; }
    public string? Education { get; set; }
    public string? Certificate { get; set; }
    public string? Introduce { get; set; }
    public string? Speacialty { get; set; }
    public Status? Status { get; set; }

    [ForeignKey("UserId")] public ApplicationUser? User { get; set; }

    public ICollection<Appointments>? Appointments { get; set; }
    public ICollection<Reviews>? Reviews { get; set; }
}

public enum Status
{
    active,
    inactive
}