namespace NewFace.DTOs.Actor;

public class GetActorImagesByGroupResponseDto
{
    public int ActorId { get; set; }
    public int GroupId { get; set; }
    public List<ActorImageDto> Images { get; set; } = new List<ActorImageDto>();
}


public class ActorImageDto
{
    public int ImageId { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int GroupOrder { get; set; }
}