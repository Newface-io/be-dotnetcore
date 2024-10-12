using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewFace.DTOs.Home;

public class GetActorPortfolioDetailResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    public int ActorId { get; set; }
    public bool IsBookMarkeeByUser { get; set; } = false;

    public List<LinkPortfolioDetailResponseDto> ActorLinks { get; set; } = new List<LinkPortfolioDetailResponseDto>();
    public List<EducationPortfolioDetailResponseDto> ActorEducations { get; set; } = new List<EducationPortfolioDetailResponseDto>();
    public List<ExperiencePortfolioDetailResponseDto> ActorExperiences { get; set; } = new List<ExperiencePortfolioDetailResponseDto>();
    public List<GetActorImages> ActorImages { get; set; } = new List<GetActorImages>();
    public List<DemoStarPortfolioDetailResponseDto> ActorDemoStars { get; set; } = new List<DemoStarPortfolioDetailResponseDto>();
}
public class LinkPortfolioDetailResponseDto
{
    public int LinkId { get; set; }
    public string Category { get; set; } = string.Empty; // SNS Link]
    public string Url { get; set; } = string.Empty;
}

public class EducationPortfolioDetailResponseDto
{
    public int EducationId { get; set; } 
    public string EducationType { get; set; } = string.Empty; // 학교 구분
    public string GraduationStatus { get; set; } = string.Empty; // 졸업 상태
    public string School { get; set; } = string.Empty;  // 학교 이름
    public string Major { get; set; } = string.Empty; // 전공
}

public class DemoStarPortfolioDetailResponseDto
{
    public int DemoStarId { get; set; } // 데모스타 아이디
    public string Title { get; set; } = string.Empty; // 제목
    public string Category { get; set; } = string.Empty; // 작품 카테고리
    public string Url { get; set; } = string.Empty; // Url
    public bool IsLikedByUser { get; set; } = false; // LIKE
}


public class ExperiencePortfolioDetailResponseDto
{
    public int ExperienceId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string WorkTitle { get; set; } = string.Empty;
}