using NewFace.DTOs.Auth;
using NewFace.Models.User;
using NewFace.Responses;

namespace NewFace.Services;

public interface IAuthService
{
    int? GetUserIdFromToken();
    Task<ServiceResponse<SignInResponseDto>> SignUpWithExternalProvider(SignUpWithExternalProviderRequestDto request);
    Task<ServiceResponse<int>> SignUpEmail(SignUpEmailRequestDto request);
    Task<ServiceResponse<SignInResponseDto>> SignInEmail(SignInRequestDto request);

    #region naver
    string GetNaverLoginUrl();
    Task<ServiceResponse<string>> GetNaverToken(string code);
    Task<ServiceResponse<NaverUserInfoResponseDto>> GetNaverUserInfo(string accessToken);
    
    #endregion

    #region kakao
    string GetKakaoLoginUrl();
    Task<ServiceResponse<string>> GetKakaoToken(string code);
    Task<ServiceResponse<KakaoUserInfoResponseDto>> GetKakaoUserInfo(string accessToken);
    #endregion

    Task<ServiceResponse<SignInResponseDto>> SignInWithExternalProvider(string loginType, string id);
    Task<ServiceResponse<IsCompletedResponseDto>> IsCompleted(string id, string signinType); // only 간편 로그인
    string CreateHashPassword(string password);
    bool VerifyPassword(string enteredPassword, string storedHash);
    string GenerateJwtToken(User user, string role, int roleSpecificId = 0);
    Task<ServiceResponse<bool>> SendOTP(string phone);
    Task<ServiceResponse<bool>> VerifyOTP(string phone, string inputOTP);

}
