using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Actor;

public class ActorExperience
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    [JsonIgnore]
    public virtual Actor Actor { get; set; } = null!;

    [Required]
    [StringLength(10)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string WorkTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Role { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}