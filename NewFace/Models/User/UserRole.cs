using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models.User;

public class UserRole
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
