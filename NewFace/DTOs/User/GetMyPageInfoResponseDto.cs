using NewFace.Models;

namespace NewFace.DTOs.User;

public class ActorMyPageInfoDto : IGetMyPageInfoResponseDto
{
    public int UserId { get; set; }
    public int ActorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class EnterMyPageInfoDto : IGetMyPageInfoResponseDto
{
    public int UserId { get; set; }
    public int EnterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class CommonMyPageInfoDto : IGetMyPageInfoResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}