namespace NewFace.DTOs.User;



public class ActorUserInfoForEditDto : IGetUserInfoForEditResponseDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EnterUserInfoForEditDto : IGetUserInfoForEditResponseDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string CeoName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string BusinessLicenseImagePublicUrl { get; set; } = string.Empty;
    public string BusinessCardImagePublicUrl { get; set; } = string.Empty;
}


public class CommonUserInfoForEditDto : IGetUserInfoForEditResponseDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}