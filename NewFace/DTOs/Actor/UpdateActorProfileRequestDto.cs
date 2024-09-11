using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class UpdateActorProfileRequestDto
{

    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // user

    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty; // user

    public string Gender { get; set; } = string.Empty; // actor
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; } // actor
    [Range(0, 300)]
    public decimal? Height { get; set; } // actor
    [Range(0, 500)]
    public decimal? Weight { get; set; } // actor

    [StringLength(1000)]
    public string Bio { get; set; } = string.Empty; // actor

    public List<AddActorExperienceRequestDto>? Experiences { get; set; }
    public List<AddActorEducationRequestDto>? Education { get; set; }
    public List<AddActorLinkRequestDto>? Links { get; set; }
}
