namespace NewFace.DTOs.User;

public interface IGetMyPageInfoResponseDto
{
    int UserId { get; set; }
    string Name { get; set; }
    string Email { get; set; }
    string Phone { get; set; }
}
