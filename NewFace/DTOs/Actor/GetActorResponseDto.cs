using NewFace.Models.Actor;

namespace NewFace.DTOs.Actor;

public class GetActorResponseDto
{
    // user 
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // actor
    public string BirthDate { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    public List<ActorEducation> ActorEducations { get; set; } = new List<ActorEducation>();
    public List<ActorExperience> ActorExperiences { get; set; } = new List<ActorExperience>();
    public List<ActorLink> ActorLinks { get; set; } = new List<ActorLink>();
}
