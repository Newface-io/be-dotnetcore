using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models.Actor;

public class ActorEducation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    public virtual Actor Actor { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string EducationType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string GraduationStatus { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string School { get; set; } = string.Empty;

    [StringLength(100)]
    public string Major { get; set; } = string.Empty;

}