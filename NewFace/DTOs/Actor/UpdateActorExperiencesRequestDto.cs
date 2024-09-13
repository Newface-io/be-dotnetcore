using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class UpdateActorExperiencesRequestDto
{
    public List<UpdateActorExperiences> UpdateActorExperiences { get; set; } = new List<UpdateActorExperiences>();
}

public class UpdateActorExperiences
{

    // 0: add | 숫자: update | no: delete
    public int ExperienceId { get; set; } = 0;
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string WorkTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Role { get; set; } = string.Empty;

    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}