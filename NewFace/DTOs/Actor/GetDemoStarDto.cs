namespace NewFace.DTOs.Actor;

public class ActorDemoStarListDto
{
    public int ActorId { get; set; }
    public List<DemoStarDto> DemoStars { get; set; } = new List<DemoStarDto>();
}

public class DemoStarDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}