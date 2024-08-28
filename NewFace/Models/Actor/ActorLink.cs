using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models.Actor;

public class ActorLink
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    public virtual Actor Actor { get; set; } = null!;

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }
}
