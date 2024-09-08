using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Actor;

public class ActorLink
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    [JsonIgnore]
    public virtual Actor Actor { get; set; } = null!;

    [Required]
    public string Category { get; set; } = string.Empty; // SNS Link

    [Required]
    public string Url { get; set; } = string.Empty;
}
