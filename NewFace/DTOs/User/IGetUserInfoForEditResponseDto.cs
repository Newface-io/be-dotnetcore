namespace NewFace.DTOs.User;

public interface IGetUserInfoForEditResponseDto
{
    string Name { get; set; }
    DateTime? BirthDate { get; set; }
    string Gender { get; set; }
    string Phone { get; set; }
    string Email { get; set; }
}
