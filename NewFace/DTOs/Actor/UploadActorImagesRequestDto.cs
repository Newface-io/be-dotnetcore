namespace NewFace.DTOs.Actor;

public class UploadActorImagesRequestDto
{
    public string Title { get; set; } = string.Empty;
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
}
