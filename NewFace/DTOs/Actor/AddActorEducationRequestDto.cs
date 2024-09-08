using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class AddActorEducationRequestDto
{
    [StringLength(50)]
    public string EducationType { get; set; } = string.Empty; // 학교 구분

    [StringLength(50)]
    public string GraduationStatus { get; set; } = string.Empty; // 졸업 상태

    [StringLength(255)]
    public string School { get; set; } = string.Empty; // 학교 이름

    [StringLength(100)]
    public string Major { get; set; } = string.Empty; // 전공
}
