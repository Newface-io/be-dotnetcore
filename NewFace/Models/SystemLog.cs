using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewFace.Models;

public class SystemLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string LogLevel { get; set; } = "Error"; // 예: "Error", "Warning", "Information"

    [StringLength(255)]
    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; } // 예외 정보가 있을 경우 저장

    public string? AdditionalData { get; set; } // 추가적인 데이터나 컨텍스트 정보 ( ex: UserId: 12345, IP: 192.168.1.1 )
}