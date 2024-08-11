using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Auth;

public class TermAgreementDto
{
    [Required]
    public int Code { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public bool IsAgreed { get; set; }
}