using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NewFace.Models.Actor;

public class ActorImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Actor")]
    public int ActorId { get; set; }
    [JsonIgnore]
    public virtual Actor Actor { get; set; } = null!;

    public int GroupId { get; set; }        // 그룹 이미지 구분을 위한 데이터
    public int GroupOrder { get; set; }     // 그룹 내에서의 순서
    public int ActorOrder { get; set; }     // 배우의 전체 이미지 중 순서
    [StringLength(255)]
    public string StoragePath { get; set; } = string.Empty; // 실제 저장 경로 (예: "actors/12345.jpg")
    public string PublicUrl { get; set; } = string.Empty;   // 공개 접근 가능한 URL

    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    [StringLength(10)]
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public DateTime DeletedDate { get; set; }

}
