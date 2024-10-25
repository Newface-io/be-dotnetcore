using NewFace.DTOs.Auth;
using NewFace.Models.User;
using NewFace.Responses;

namespace NewFace.Services;

public interface IAuthService
{
    int? GetUserIdFromToken();
    Task<ServiceResponse<int>> SignUpEmail(SignUpRequestDto request);
    Task<ServiceResponse<SignInResponseDto>> SignInEmail(SignInRequestDto request);
    string GetKakaoLoginUrl();
    Task<string> GetKakaoToken(string code);
    Task<ServiceResponse<KakaoUserInfoResponseDto>> GetKakaoUserInfo(string accessToken);
    string CreateHashPassword(string password);
    bool VerifyPassword(string enteredPassword, string storedHash);
    string GenerateJwtToken(User user, string role, int roleSpecificId = 0);
    Task<ServiceResponse<bool>> SendOTP(string phone);
    Task<ServiceResponse<bool>> VerifyOTP(string phone, string inputOTP);

}
