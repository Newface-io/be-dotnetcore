using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Home;

public class GetDemoStarResponseDto
{
    public int actorId {  get; set; }
    public string actorName { get; set; } = string.Empty;
    public string actorImageUrl { get; set; } = string.Empty;
    public DemoStarData demoStarData { get; set; } = new DemoStarData();
    public List<RecommendedDemoStarDto> RecommendedDemoStars { get; set; } = new List<RecommendedDemoStarDto>();
}


public class DemoStarData
{
    public int demoStarId { get; set; } // 데모스타 아이디
    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // 제목
    [StringLength(10)]
    public string Category { get; set; } = string.Empty; // 작품 카테고리
    public string Url { get; set; } = string.Empty; // Url
    public long ViewCount { get; set; } = 0; // 조회수
    public int LikesCount { get; set; } = 0; // 전체 좋아요 개수
    public bool IsLikedByUser { get; set; } = false; // Like 유무
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class RecommendedDemoStarDto
{
    public int actorId { get; set; }
    public string actorImageUrl { get; set; } = string.Empty;
    public int demoStarId { get; set; }
    public string title { get; set; } = string.Empty;
    public string url { get; set; } = string.Empty;
}