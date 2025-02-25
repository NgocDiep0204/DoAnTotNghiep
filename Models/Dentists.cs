using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Dentists
{
    public required string DentistId { get; set; }
    public required string UserId { get; set; }
    public string Speacialty { get; set; }
    [ForeignKey("UserId")]
    public ApplicationUser Users { get; set; }
    public ICollection<Appointments> Appointments { get; set; }
}