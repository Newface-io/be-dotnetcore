using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Home;

public class GetDemoStarResponseDto
{
    public int actorId {  get; set; }
    public string actorName { get; set; } = string.Empty;
    public DemoStarData demoStarData { get; set; } = new DemoStarData();
}


public class DemoStarData
{
    public int demoStarId { get; set; } // 데모스타 아이디
    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // 제목
    [StringLength(10)]
    public string Category { get; set; } = string.Empty; // 작품 카테고리
    public string Url { get; set; } = string.Empty; // Url
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}