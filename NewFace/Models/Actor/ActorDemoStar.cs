using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Actor;

public class ActorDemoStar
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    [JsonIgnore]
    public virtual Actor Actor { get; set; } = null!;

    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // 제목
    [StringLength(10)]
    public string Category { get; set; } = string.Empty; // 작품 카테고리
    public string Url { get; set; } = string.Empty; // Url
    public long ViewCount { get; set; } = 0; // 조회수
    public int LikesFromActors { get; set; } = 0; // 배우로부터 '좋아요'
    public int LikesFromCommons { get; set; } = 0; // 일반유저로부터 '좋아요'
    public int LikesFromEnters { get; set; } = 0; // 엔터로부터 '좋아요'
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
