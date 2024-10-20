using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Auth;

public class KakaoUserInfoResponseDto
{
    public long Id { get; set; }
    public string ConnectedAt { get; set; }
}
