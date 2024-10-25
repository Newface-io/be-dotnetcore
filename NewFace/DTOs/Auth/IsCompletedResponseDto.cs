namespace NewFace.DTOs.Auth;

public class IsCompletedResponseDto
{
    public string id { get; set; } = string.Empty;
    public string loginType { get; set; } = string.Empty;
    public bool isCompleted { get; set; } = false;
}