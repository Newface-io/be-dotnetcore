using NewFace.Models;
using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Auth;

public class SignUpRequestDto
{

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name must be 100 characters or less.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(100, ErrorMessage = "Email must be 100 characters or less.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(20, ErrorMessage = "Password must be 20 characters or less.")]
    public string Password { get; set; }

    [StringLength(20, ErrorMessage = "Phone number must be 20 characters or less.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "You must agree to the terms.")]
    public List<TermAgreementDto> TermsAgreements { get; set; } = new List<TermAgreementDto>();
}