namespace NewFace.DTOs.User;

public class UpdateUserInfoForEditResponseDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
