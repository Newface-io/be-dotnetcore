using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class GetActorExperiencesResponseDto
{
    public int ActorId { get; set; }
    public List<ActorExperiences> ActorExperiences { get; set; } = new List<ActorExperiences>();
}

public class ActorExperiences
{
    public int ExperienceId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string WorkTitle { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedDate { get; set; } 
    public DateTime LastUpdated { get; set; } 
}
