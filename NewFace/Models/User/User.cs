﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models.User;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    //[Required]
    //public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; } // 생년월일

    [StringLength(5)]
    public string Gender { get; set; } = string.Empty; // 성별
    [StringLength(255)]
    public string StoragePath { get; set; } = string.Empty; // 실제 저장 경로 (예: "actors/12345.jpg")
    public string PublicUrl { get; set; } = string.Empty;   // 공개 접근 가능한 URL

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;

    public virtual UserAuth UserAuth { get; set; } = new UserAuth();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public ICollection<UserLike> Likes { get; set; } = new List<UserLike>();
    public virtual ICollection<Term> TermsAgreements { get; set; } = new HashSet<Term>();
    public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();
    public virtual Actor.Actor? Actor { get; set; }
    public virtual Entertainment.Entertainment? EntertainmentProfessional { get; set; }

}
