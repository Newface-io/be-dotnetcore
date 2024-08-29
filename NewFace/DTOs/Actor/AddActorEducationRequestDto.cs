using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class AddActorEducationRequestDto
{
    [StringLength(50)]
    public string EducationType { get; set; } = string.Empty;

    [StringLength(50)]
    public string GraduationStatus { get; set; } = string.Empty;

    [StringLength(255)]
    public string School { get; set; } = string.Empty;

    [StringLength(100)]
    public string Major { get; set; } = string.Empty;
}
