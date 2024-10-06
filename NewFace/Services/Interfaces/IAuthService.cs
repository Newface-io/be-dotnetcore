using NewFace.DTOs.Auth;
using NewFace.Models.User;
using NewFace.Responses;

namespace NewFace.Services;

public interface IAuthService
{
    int? GetUserIdFromToken();
    Task<ServiceResponse<int>> SignUp(SignUpRequestDto request);
    Task<ServiceResponse<SignInResponseDto>> SignIn(SignInRequestDto request);
    string CreateHashPassword(string password);
    bool VerifyPassword(string enteredPassword, string storedHash);
    string GenerateJwtToken(User user, string role, int roleSpecificId = 0);
    Task<ServiceResponse<int>> SendOTP(int userId, string phone);
    Task<ServiceResponse<int>> VerifyOTP(int userId, string inputOTP);

}
