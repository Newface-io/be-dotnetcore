namespace NewFace.DTOs.Home;

public class GetMainPageResponseDto
{
    public List<ActorTopDto> TopActorData { get; set; } = new List<ActorTopDto>();
    public DemoStarDataResponseDto DemoStarData { get; set; } = new DemoStarDataResponseDto();

}

public class ActorTopDto
{
    public int ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string MainActorImageUrl { get; set; } = string.Empty;
}

public class DemoStarDataResponseDto
{
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; } = 20;
    public List<DemoStarItemDto> DamoStars { get; set; } = new List<DemoStarItemDto>();
}

public class DemoStarItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long ViewCount { get; set; } = 0;
    public int LikesCount {  get; set; } = 0;
    public bool IsLikedByUser { get; set; } = false;
}