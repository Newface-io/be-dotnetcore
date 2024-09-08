using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Actor;

public class ActorEducation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    [JsonIgnore]
    public virtual Actor Actor { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string EducationType { get; set; } = string.Empty; // 학교 구분

    [Required]
    [StringLength(50)]
    public string GraduationStatus { get; set; } = string.Empty; // 졸업 상태

    [Required]
    [StringLength(255)]
    public string School { get; set; } = string.Empty;  // 학교 이름

    [StringLength(100)]
    public string Major { get; set; } = string.Empty; // 전공

}