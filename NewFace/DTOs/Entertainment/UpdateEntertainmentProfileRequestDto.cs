using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class UpdateEntertainmentProfileRequestDto
{
    public int UserId { get; set; }

    [StringLength(50)]
    public string CompanyType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100)]
    public string CeoName { get; set; } = string.Empty;

    [StringLength(255)]
    public string CompanyAddress { get; set; } = string.Empty;

    [StringLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(20)]
    public string ContactPhone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string ContactEmail { get; set; } = string.Empty;

    [StringLength(100)]
    public string ContactDepartment { get; set; } = string.Empty;

    [StringLength(100)]
    public string ContactPosition { get; set; } = string.Empty;
    public IFormFile? BusinessLicenseImage { get; set; }
    public IFormFile? BusinessCardImage { get; set; }
    public bool isUpdatedImage { get; set; } = false;
}