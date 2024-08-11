using NewFace.DTOs.Auth;
using NewFace.Models;
using NewFace.Responses;

namespace NewFace.Services;

public interface IAuthService
{
    Task<ServiceResponse<int>> Register(RegisterRequestDto request);
    Task<ServiceResponse<LoginResponseDto>> Login(LoginRequestDto request);
    string CreateHashPassword(string password);
    bool VerifyPassword(string enteredPassword, string storedHash);
    string GenerateJwtToken(User user);
}
