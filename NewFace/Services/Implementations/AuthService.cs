using Azure.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Auth;
using NewFace.Models;
using NewFace.Responses;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace NewFace.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;
    private readonly IDistributedCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(DataContext context, IConfiguration configuration, ILogService logService, IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logService = logService;
        _configuration = configuration;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
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

    public async Task<ServiceResponse<int>> SignUp(SignUpRequestDto request)
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
                PasswordHash = passwordHash,
                Phone = request.Phone,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            if (!_context.Users.Local.Any(u => u.Email == user.Email))
            {
                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            // 
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

    public async Task<ServiceResponse<SignInResponseDto>> SignIn(SignInRequestDto request)
    {
        var response = new ServiceResponse<SignInResponseDto>();

        try
        {
            var token = string.Empty;

            var user = await _context.Users
                            .Include(u => u.UserRoles)
                            .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 1. check email
            if (user == null)
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
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            // 3. get role
            var userRole = user.UserRoles.FirstOrDefault()?.Role ?? string.Empty;

            // 6.Prepare response data
            response.Data = new SignInResponseDto()
            {
                id = user.Id,
                Email = user.Email,
                role = userRole
            };

            // 4. Set specific ID based on role
            switch (userRole)
            {
                case NewFace.Common.Constants.UserRole.Actor:
                    response.Data.actorId = await _context.Actors
                        .Where(a => a.UserId == user.Id)
                        .Select(a => a.Id)
                        .FirstOrDefaultAsync();

                    // 5. Generate JWT token
                    token = GenerateJwtToken(user, userRole, response.Data.actorId??0);

                    break;
                case NewFace.Common.Constants.UserRole.Entertainment:
                    response.Data.enterId = await _context.Entertainments
                        .Where(e => e.UserId == user.Id)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    // 5. Generate JWT token
                    token = GenerateJwtToken(user, userRole, response.Data.enterId??0);
                    break;
                default:
                    // 5. Generate JWT token
                    token = GenerateJwtToken(user, userRole);
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

            _logService.LogError("EXCEPTION: SignIn", ex.Message, "ip: ");

            return response;
        }

    }

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
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, role),
        };

        switch (role)
        {
            case NewFace.Common.Constants.UserRole.Actor:
                if (roleSpecificId != 0)
                {
                    claims.Add(new Claim("ActorId", roleSpecificId.ToString()));
                }
                break;
            case NewFace.Common.Constants.UserRole.Entertainment:
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

    public async Task<ServiceResponse<int>> SendOTP(int userId, string phone)
    {
        var response = new ServiceResponse<int>();

        try
        {
            string accountSid = _configuration["Redis:AccountSid"];
            string authToken = _configuration["Redis:AuthToken"];
            string sendNumber = _configuration["Redis:SendNumber"];

            TwilioClient.Init(accountSid, authToken);

            phone = Helpers.Common.FormatPhoneNumber(phone);

            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString("D6");

            var message = MessageResource.Create(
                body: "[NewFace] 인증번호는 " + otp + " 입니다.",
                from: new Twilio.Types.PhoneNumber(sendNumber),
                to: new Twilio.Types.PhoneNumber(phone)
            );

            if (message.Status == MessageResource.StatusEnum.Queued ||
                message.Status == MessageResource.StatusEnum.Sent ||
                message.Status == MessageResource.StatusEnum.Delivered)
            {
                // save OTP on Redis
                string cacheKey = $"OTP:{userId}";
                await _cache.SetStringAsync(cacheKey, otp, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                response.Data = userId;
                response.Success = true;

                return response;
            }
            else
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.SMS_SEND_FAILED.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.SMS_SEND_FAILED];

                return response;

            }

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = 0;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SendOTP", ex.Message, "ip: ");

            return response;
        }
    }


    public async Task<ServiceResponse<int>> VerifyOTP(int userId, string inputOTP)
    {
        var response = new ServiceResponse<int>();

        try
        {
            string cacheKey = $"OTP:{userId}";
            string storedOTP = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(storedOTP))
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.OTP_NOT_FOUND.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.OTP_NOT_FOUND];

                return response;
            }

            if (inputOTP == storedOTP)
            {
                await _cache.RemoveAsync(cacheKey);

                response.Data = userId;
                response.Success = true;

                return response;
            }
            else
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.OTP_MISMATCH.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.OTP_MISMATCH];

                return response;
            }

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = 0;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: VerifyOTP", ex.Message, "ip: ");

            return response;
        }
    }


}
