using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models;

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

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(5)]
    public string Gender { get; set; } = string.Empty; // 성별

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Term> TermsAgreements { get; set; } = new HashSet<Term>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();
    public virtual Models.Actor.Actor Actor { get; set; } = new Actor.Actor();
    public virtual Models.Entertainment.Entertainment EntertainmentProfessional { get; set; } = new Entertainment.Entertainment();

}
