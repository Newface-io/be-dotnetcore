namespace NewFace.DTOs.Home;

public class GetActorPortfolioResponseDto
{
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; } = 50;
    public List<ActorPortfolioDto> Actors { get; set; } = new List<ActorPortfolioDto>();
}

public class ActorPortfolioDto
{
    public int ActorId { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Age { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    public int BookMarksCount { get; set; } = 0; // 북마크 개수
    public bool IsBookMarkeeByUser { get; set; } = false; // BookMark 유무
}
