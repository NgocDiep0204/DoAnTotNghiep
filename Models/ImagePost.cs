using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class ImagePost
{
    [Key]public string ImgId { get; set; }
    public string? PostId { get; set; }
    public string? ImageUrl { get; set; }
    [ForeignKey("PostId")]
    public Posts? Post { get; set; }
}