using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Actor;

public class Actor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    [StringLength(255)]
    public string Address { get; set; } = string.Empty; // 주소

    [Precision(5, 2)]
    public decimal? Height { get; set; } // 키
    [Precision(5, 2)]
    public decimal? Weight { get; set; } // 몸무게

    public string Bio { get; set; } = string.Empty; // 자기소개
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public virtual ICollection<ActorExperience> Experiences { get; set; } = new HashSet<ActorExperience>(); // 경력사항
    public virtual ICollection<ActorEducation> Education { get; set; } = new HashSet<ActorEducation>(); // 학력사항
    public virtual ICollection<ActorLink> Links { get; set; } = new HashSet<ActorLink>(); // SNS 링크
    public virtual ICollection<ActorDemoStar> DemoStars { get; set; } = new HashSet<ActorDemoStar>(); // 데모스타
    public virtual ICollection<ActorImage> Images { get; set; } = new HashSet<ActorImage>(); // 이미지
}