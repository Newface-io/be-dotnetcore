using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class AddActorLinkRequestDto
{
    public string Url { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }
}
