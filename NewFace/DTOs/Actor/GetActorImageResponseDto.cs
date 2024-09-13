namespace NewFace.DTOs.Actor;

public class GetActorImagesResponseDto
{
    public int ActorId { get; set; }
    public List<GetActorImages> Images { get; set; } = new List<GetActorImages>();
}

public class GetActorImages
{
    public int ImageId { get; set; } // ActorImage table PK
    public string PublicUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public int GroupOrder { get; set; }
    public int ImageCount { get; set; }
    public bool IsMainImage { get; set; }
}
