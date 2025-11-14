using System.ComponentModel.DataAnnotations;

namespace BookDb.Models
{
 public class Author
 {
 [Key]
 public int Id { get; set; }

 [Required, MaxLength(200)]
 public string Name { get; set; } = string.Empty;

 public string? Bio { get; set; }
 }
}
