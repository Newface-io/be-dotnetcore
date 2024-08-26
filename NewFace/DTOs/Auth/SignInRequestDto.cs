using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Auth;

public class SignInRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(100, ErrorMessage = "Email must be 100 characters or less.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(20, ErrorMessage = "Password must be 20 characters or less.")]
    public string Password { get; set; } = string.Empty;
}