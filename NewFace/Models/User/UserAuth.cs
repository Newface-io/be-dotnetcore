using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models.User;

public class UserAuth
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(20)]
    public string AuthKey { get; set; } = string.Empty;
    [Required]
    public string AuthValue { get; set; } = string.Empty;

    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
