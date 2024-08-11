using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models;

public class Term
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    [Required]
    public int Code { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public bool IsAgreed { get; set; }

    [Required]
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    // User와의 관계 설정 (Foreign Key)
    [ForeignKey("UserId")]
    public User User { get; set; }
}
