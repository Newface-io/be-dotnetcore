using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Auth;

public class SignInResponseDto
{
    public int id { get; set; }
    public string email { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
    public string imageUrl { get; set; } = string.Empty;
    public string role { get; set; } = string.Empty;
    public int? actorId { get; set; }  
    public int? enterId { get; set; } 
}
