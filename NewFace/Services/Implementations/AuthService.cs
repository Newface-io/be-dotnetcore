using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Auth;
using NewFace.Models.User;
using NewFace.Responses;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace NewFace.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IDistributedCache _cache;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _jwtSecretKey;
    private readonly string _redisAccountSid;
    private readonly string _jwtAuthToken;
    private readonly string _jwtSendNumber;

    private readonly string _kakaoClientId;
    private readonly string _kakaoClientSecret;
    private readonly string _kakaoRedirectUri;
    private readonly string _kakaoAuthUrl;

    private readonly string _naverClinetID;
    private readonly string _naverClientSecret;
    private readonly string _naverRedirectURI;

    public AuthService(DataContext context, ILogService logService, IDistributedCache cache, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logService = logService;
        _cache = cache;

        _httpClientFactory = httpClientFactory; // 외부 api 호출을 위해
        _httpContextAccessor = httpContextAccessor; // 현재 요청과 관련된 정보(현재 HTTP 요청의 컨텍스트에 접근) 

        _jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? string.Empty;
        _redisAccountSid = Environment.GetEnvironmentVariable("REDIS_ACCOUNT_SID") ?? string.Empty;
        _jwtAuthToken = Environment.GetEnvironmentVariable("REDIS_AUTH_TOKEN") ?? string.Empty;
        _jwtSendNumber = Environment.GetEnvironmentVariable("REDIS_SEND_NUMBER") ?? string.Empty;

        _naverClinetID = Environment.GetEnvironmentVariable("NAVER_CLIENT_ID") ?? string.Empty;
        _naverClientSecret = Environment.GetEnvironmentVariable("NAVER_CLIENT_SECRET") ?? string.Empty;
        _naverRedirectURI = Environment.GetEnvironmentVariable("NAVER_REDIRECT_URI_LOCAL") ?? string.Empty;

        _kakaoClientId = Environment.GetEnvironmentVariable("KAKAO_CLIENT_ID") ?? string.Empty;
        _kakaoClientSecret = Environment.GetEnvironmentVariable("KAKAO_CLIENT_SECRET") ?? string.Empty;
        _kakaoRedirectUri = Environment.GetEnvironmentVariable("KAKAO_REDIRECT_URI_LOCAL") ?? string.Empty;
        _kakaoAuthUrl = Environment.GetEnvironmentVariable("KAKAO_AUTH_URL") ?? string.Empty;
    }

    public int? GetUserIdFromToken()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        return null;
    }

    public async Task<ServiceResponse<SignInResponseDto>> SignUpWithExternalProvider(SignUpWithExternalProviderRequestDto request)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users
                .Include(u => u.UserAuth)
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserAuth.AuthKey == request.LoginType && u.UserAuth.AuthValue == request.Id);

            if (user == null || user.UserAuth == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            }

            user.Name = request.Name;
            user.Phone = request.Phone;

            user.UserAuth.IsCompleted = true;

            foreach (var termDto in request.TermsAgreements)
            {
                var term = new Term
                {
                    UserId = user.Id,
                    Code = termDto.Code,
                    Name = termDto.Name,
                    IsAgreed = termDto.IsAgreed,
                    LastUpdated = DateTime.Now
                };
                _context.Terms.Add(term);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();


            var token = string.Empty;

            response = await SignInWithExternalProvider(request.LoginType, request.Id);

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Data = new SignInResponseDto();
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SignUp", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<int>> SignUpEmail(SignUpEmailRequestDto request)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.REGISTERED_EMAIL.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.REGISTERED_EMAIL];

                return response;
            }

            var passwordHash = CreateHashPassword(request.Password);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                CreatedDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            if (!_context.Users.Local.Any(u => u.Email == user.Email))
            {
                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            var userAuth = new UserAuth
            {
                UserId = user.Id,
                AuthKey = USER_AUTH.EMAIL,
                AuthValue = passwordHash,
                IsCompleted = true,
                UpdatedDate = DateTime.Now,
            };

            _context.UserAuth.Add(userAuth);

            foreach (var termDto in request.TermsAgreements)
            {
                var term = new Term
                {
                    UserId = user.Id,
                    Code = termDto.Code,
                    Name = termDto.Name,
                    IsAgreed = termDto.IsAgreed,
                    LastUpdated = DateTime.Now
                };
                _context.Terms.Add(term);
            }

            await _context.SaveChangesAsync(); 

            await transaction.CommitAsync();

            response.Data = user.Id;
            response.Success = true;

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Data = 0;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SignUp", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<SignInResponseDto>> SignInEmail(SignInRequestDto request)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        try
        {
            var token = string.Empty;

            var user = await _context.Users
                            .Include(u => u.UserAuth)
                            .Include(u => u.UserRoles)
                            .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 1. check email
            if (user == null || user.UserAuth == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            } else if (user.IsDeleted)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.DELETED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.DELETED_USER];

                return response;
            }

            // 2. check password
            if (!VerifyPassword(request.Password, user.UserAuth.AuthValue))
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            response = await GetSignInData(user);

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SignIn", ex.Message, "ip: ");

            return response;
        }

    }

    #region naver

    private async Task<ServiceResponse<SignInResponseDto>> GetSignInData(User user)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        var token = string.Empty;

        try
        {
            // 3. get role
            var userRole = user.UserRoles.FirstOrDefault()?.Role ?? string.Empty;

            // 4.Prepare response data
            response.Data = new SignInResponseDto()
            {
                id = user.Id,
                email = user.Email,
                name = user.Name,
                imageUrl = user.PublicUrl,
                role = userRole
            };

            // 5. Set specific ID based on role
            switch (userRole)
            {
                case NewFace.Common.Constants.USER_ROLE.ACTOR:
                    response.Data.actorId = await _context.Actors
                        .Where(a => a.UserId == user.Id)
                        .Select(a => a.Id)
                        .FirstOrDefaultAsync();

                    // 5. Generate JWT token
                    token = GenerateJwtToken(user, userRole, response.Data.actorId ?? 0);

                    break;
                case NewFace.Common.Constants.USER_ROLE.ENTER:
                    response.Data.enterId = await _context.Entertainments
                        .Where(e => e.UserId == user.Id)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    // 5. Generate JWT token
                    token = GenerateJwtToken(user, userRole, response.Data.enterId ?? 0);
                    break;
                case NewFace.Common.Constants.USER_ROLE.COMMON:
                    // 5. Generate JWT token
                    token = GenerateJwtToken(user, userRole);
                    break;
                default:
                    token = string.Empty;
                    break;
            }

            response.Data.token = token;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SetSignInData", ex.Message, "ip: ");

            return response;
        }

    }

    public string GetNaverLoginUrl()
    {
        var clientId = _naverClinetID;
        var redirectUri = _naverRedirectURI;
        return $"https://nid.naver.com/oauth2.0/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}";
    }

    public async Task<ServiceResponse<string>> GetNaverToken(string code)
    {
        var response = new ServiceResponse<string>();

        try
        {
            using var client = _httpClientFactory.CreateClient();

            var clientId = _naverClinetID;
            var clientSecret = _naverClientSecret;
            var redirectUri = _naverRedirectURI;

            var responseFromNaver = await client.PostAsync($"https://nid.naver.com/oauth2.0/token?grant_type=authorization_code&client_id={clientId}&client_secret={clientSecret}&code={code}&redirect_uri={redirectUri}", null);

            var content = await responseFromNaver.Content.ReadFromJsonAsync<JsonElement>();

            if (content.GetProperty("access_token").GetString() != null)
            {
                response.Data = content.GetProperty("access_token").GetString();
            }
            else
            {
                response.Success = false;
                response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            }

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetNaverToken", ex.Message, "ip: ");

            return response;
        }

    }

    public async Task<ServiceResponse<NaverUserInfoResponseDto>> GetNaverUserInfo(string accessToken)
    {
        var response = new ServiceResponse<NaverUserInfoResponseDto>();

        try
        {
            using var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var responseFromNaver = await client.GetFromJsonAsync<JsonElement>("https://openapi.naver.com/v1/nid/me");

            if (responseFromNaver.TryGetProperty("response", out JsonElement userInfo))
            {
                response.Data = new NaverUserInfoResponseDto
                {
                    id = userInfo.GetProperty("id").GetString(),
                    email = userInfo.GetProperty("email").GetString(),
                    name = userInfo.GetProperty("name").GetString()
                };

            }
            else
            {
                response.Success = false;
                response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            }

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetNaverUserInfo", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<IsCompletedResponseDto>> IsCompleted(string id, string signinType)
    {
        var response = new ServiceResponse<IsCompletedResponseDto>();

        try
        {
            var userAuth = await _context.UserAuth
                                    .FirstOrDefaultAsync(ua => ua.AuthKey == signinType && ua.AuthValue == id);

            if (userAuth == null)
            {
                var newUser = new User();

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                userAuth = new UserAuth
                {
                    UserId = newUser.Id,
                    AuthKey = signinType,
                    AuthValue = id,
                    IsCompleted = false
                };

                _context.UserAuth.Add(userAuth);
                await _context.SaveChangesAsync();

                response.Data = new IsCompletedResponseDto()
                {
                    id = id,
                    loginType = signinType,
                    isCompleted = false
                };
            }
            else
            {
                response.Data = new IsCompletedResponseDto()
                {
                    id = id,
                    loginType = signinType,
                    isCompleted = userAuth.IsCompleted
                };
            }

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = new IsCompletedResponseDto();
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: IsCompleted", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<SignInResponseDto>> SignInWithExternalProvider(string loginType, string id)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        try
        {
            var token = string.Empty;

            var userAuth = await _context.UserAuth.FirstOrDefaultAsync(x => x.AuthKey == loginType && x.AuthValue == id);

            // 0. check user auth
            if (userAuth == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            }

            var user = await _context.Users
                            .Include(u => u.UserAuth)
                            .Include(u => u.UserRoles)
                            .FirstOrDefaultAsync(u => u.Id == userAuth.UserId);

            // 1. check email
            if (user == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            }
            else if (user.IsDeleted)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.DELETED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.DELETED_USER];

                return response;
            }

            response = await GetSignInData(user);

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SignInWithExternalProvider", ex.Message, "ip: ");

            return response;
        }
    }

    #endregion

    #region kakao
    public string GetKakaoLoginUrl()
    {
        return $"{_kakaoAuthUrl}?client_id={_kakaoClientId}&redirect_uri={_kakaoRedirectUri}&response_type=code";
    }

    public async Task<ServiceResponse<string>> GetKakaoToken(string code)
    {
        var result = new ServiceResponse<string>();

        try
        {
            using var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync("https://kauth.kakao.com/oauth/token",
                                new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                        {"grant_type", "authorization_code"},
                                        {"client_id", _kakaoClientId},
                                        {"client_secret", _kakaoClientSecret}, // client_secret 추가
                                        {"redirect_uri", _kakaoRedirectUri},
                                        {"code", code}
                                }));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                result.Success = false;
                result.Data = null;
                result.Code = response.StatusCode.ToString();
                result.Message = errorContent;

                _logService.LogError("EXCEPTION: GetKakaoToken", errorContent, "ip: ");
                
            } else
            {
                var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                result.Data = content.GetProperty("access_token").GetString();
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Data = null;
            result.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            result.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetKakaoToken", ex.Message, "ip: ");

            return result;
        }

    }

    public async Task<ServiceResponse<KakaoUserInfoResponseDto>> GetKakaoUserInfo(string accessToken)
    {
        var response = new ServiceResponse<KakaoUserInfoResponseDto>();

        try
        {
            using var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var responseFromKakao = await client.GetFromJsonAsync<JsonElement>("https://kapi.kakao.com/v2/user/me");

            if (responseFromKakao.TryGetProperty("id", out JsonElement idElement) &&
                responseFromKakao.TryGetProperty("connected_at", out JsonElement connectedAtElement))
            {
                response.Data = new KakaoUserInfoResponseDto()
                {
                    Id = idElement.GetInt64(),
                    ConnectedAt = connectedAtElement.GetString()
                };

                return response;
            }
            else
            {
                response.Success = false;
                return response;
            }

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetKakaoUserInfo", ex.Message, "ip: ");

            return response;
        }
    }
    #endregion

    public string CreateHashPassword(string password)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    public bool VerifyPassword(string enteredPassword, string storedHash)
    {
        byte[] hashBytes = Convert.FromBase64String(storedHash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000))
        {
            byte[] hash = pbkdf2.GetBytes(20);
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public string GenerateJwtToken(User user, string role, int roleSpecificId = 0)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, role),
        };

        switch (role)
        {
            case NewFace.Common.Constants.USER_ROLE.ACTOR:
                if (roleSpecificId != 0)
                {
                    claims.Add(new Claim("ActorId", roleSpecificId.ToString()));
                }
                break;
            case NewFace.Common.Constants.USER_ROLE.ENTER:
                if (roleSpecificId != 0)
                {
                    claims.Add(new Claim("EnterId", roleSpecificId.ToString()));
                }
                break;
            default:
                break;
        }


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<ServiceResponse<bool>> SendOTP(string phone)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            string accountSid = _redisAccountSid;
            string authToken = _jwtAuthToken;
            string sendNumber = _jwtSendNumber;

            TwilioClient.Init(accountSid, authToken);

            var formatPhone = Helpers.Common.FormatPhoneNumber(phone);

            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString("D6");

            var message = MessageResource.Create(
                body: "[NewFace] 인증번호는 " + otp + " 입니다.",
                from: new Twilio.Types.PhoneNumber(sendNumber),
                to: new Twilio.Types.PhoneNumber(formatPhone)
            );

            if (message.Status == MessageResource.StatusEnum.Queued ||
                message.Status == MessageResource.StatusEnum.Sent ||
                message.Status == MessageResource.StatusEnum.Delivered)
            {
                // save OTP on Redis
                string cacheKey = $"OTP:{phone}";
                await _cache.SetStringAsync(cacheKey, otp, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                response.Data = true;
                response.Success = true;

                return response;
            }
            else
            {
                response.Success = false;
                response.Data = false;
                response.Code = MessageCode.Custom.SMS_SEND_FAILED.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.SMS_SEND_FAILED];

                return response;

            }

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SendOTP", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<bool>> VerifyOTP(string phone, string inputOTP)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            string cacheKey = $"OTP:{phone}";
            string storedOTP = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(storedOTP))
            {
                response.Success = false;
                response.Data = false;
                response.Code = MessageCode.Custom.OTP_NOT_FOUND.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.OTP_NOT_FOUND];

                return response;
            }

            if (inputOTP == storedOTP)
            {
                await _cache.RemoveAsync(cacheKey);

                response.Data = true;
                response.Success = true;

                return response;
            }
            else
            {
                response.Success = false;
                response.Data = false;
                response.Code = MessageCode.Custom.OTP_MISMATCH.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.OTP_MISMATCH];

                return response;
            }

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: VerifyOTP", ex.Message, "ip: ");

            return response;
        }
    }

}
