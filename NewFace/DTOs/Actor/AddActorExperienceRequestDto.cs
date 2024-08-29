using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class AddActorExperienceRequestDto
{
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [StringLength(255)]
    public string WorkTitle { get; set; } = string.Empty;

    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
