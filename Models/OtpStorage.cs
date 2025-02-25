using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class OtpStorage
{
    [Key]
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string Otp { get; set; }
    public required DateTime ExpiryTime { get; set; }
    [ForeignKey("UserId")]
    public  ApplicationUser? ApplicationUser { get; set; }
}