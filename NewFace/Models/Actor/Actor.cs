using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models.Actor;

public class Actor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public DateTime? BirthDate { get; set; }

    [StringLength(255)]
    public string Address { get; set; } = string.Empty;

    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }

    public string? Bio { get; set; } // 자기소개

    public virtual ICollection<ActorExperience> Experiences { get; set; } = new HashSet<ActorExperience>(); // 경력사항
    public virtual ICollection<ActorEducation> Education { get; set; } = new HashSet<ActorEducation>(); // 학력사항
    public virtual ICollection<ActorLink> Links { get; set; } = new HashSet<ActorLink>(); // 링크
}