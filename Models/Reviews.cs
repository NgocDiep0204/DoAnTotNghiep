using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Reviews
{
    public string ReviewId { get; set; }
    public string CustomerId { get; set; }
    public string DentistId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public string CreateAt { get; set; }
    [ForeignKey("CustomerId")]
    public ApplicationUser? Customers { get; set; }
    [ForeignKey("DentistId")]
    public ApplicationUser? Dentists { get; set; }
}