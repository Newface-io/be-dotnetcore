using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NewFace.Models.User;

public class UserLike
{
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    [JsonIgnore]
    public virtual Models.User.User User { get; set; } = null!;
    public string ItemType { get; set; } = string.Empty; // e.g., "DemoStar", ...
    public int ItemId { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.Now;
}
