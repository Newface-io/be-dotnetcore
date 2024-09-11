using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class UpdateActorDemoStarDto
{
    [Required]
    public int Id { get; set; } // ActorDemoStar ID

    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // 제목

    [StringLength(10)]
    public string Category { get; set; } = string.Empty; // 작품 카테고리

    public string Url { get; set; } = string.Empty; // Url
}
