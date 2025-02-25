using Microsoft.AspNetCore.Identity;

namespace api.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Gender { get; set; } 
    public string? ImageUrl { get; set; }
    public ICollection<Appointments>? Appointments { get; set; }
    public ICollection<Reviews>? Reviews { get; set; }
}