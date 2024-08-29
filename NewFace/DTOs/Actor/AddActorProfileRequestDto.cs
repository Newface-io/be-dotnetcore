using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class AddActorProfileRequestDto
{
    public int UserId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [StringLength(255)]
    public string Address { get; set; } = string.Empty;

    [Range(0, 300)]
    public decimal? Height { get; set; }

    [Range(0, 500)]
    public decimal? Weight { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    public List<AddActorExperienceRequestDto>? Experiences { get; set; }
    public List<AddActorEducationRequestDto>? Education { get; set; }
    public List<AddActorLinkRequestDto>? Links { get; set; }
}
