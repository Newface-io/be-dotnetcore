using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Entertainment;

public class Entertainment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    [JsonIgnore]
    public virtual Models.User.User User { get; set; } = null!;

    [StringLength(50)]
    public string CompanyType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100)]
    public string CeoName { get; set; } = string.Empty;

    [StringLength(255)]
    public string CompanyAddress { get; set; } = string.Empty;

    [StringLength(100)]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(20)]
    public string ContactPhone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string ContactEmail { get; set; } = string.Empty;

    [StringLength(100)]
    public string ContactDepartment { get; set; } = string.Empty;

    [StringLength(100)]
    public string ContactPosition { get; set; } = string.Empty;
    public string BusinessLicenseImagePublicUrl { get; set; } = string.Empty;
    public string BusinessCardImagePublicUrl { get; set; } = string.Empty;
}
