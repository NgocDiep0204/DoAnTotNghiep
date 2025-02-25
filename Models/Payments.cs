using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Payments
{
    [Key]
    public string PaymentId { get; set; }
    public string AppoitmentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime StransactionDate { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    [ForeignKey("AppointmentId")]
    public Appointments Appointments { get; set; }
    
    
}

public enum PaymentStatus
{
    Pending,
    Completed,
}

public enum PaymentMethod
{
    Cash,
    Card,
    Banking
}