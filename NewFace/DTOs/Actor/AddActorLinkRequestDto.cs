using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Actor;

public class AddActorLinkRequestDto
{
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
