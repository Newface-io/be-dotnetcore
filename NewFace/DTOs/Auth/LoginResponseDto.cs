﻿using System.ComponentModel.DataAnnotations;

namespace NewFace.DTOs.Auth;

public class LoginResponseDto
{
    public int id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;

}
