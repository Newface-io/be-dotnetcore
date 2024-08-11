﻿using Azure.Core;
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

namespace NewFace.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public AuthService(DataContext context, IConfiguration configuration, ILogService logService)
    {
        _context = context;
        _logService = logService;
        _configuration = configuration;
    }

    public async Task<ServiceResponse<int>> Register(RegisterRequestDto request)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 이메일 중복 확인
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.REGISTERED_EMAIL.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.REGISTERED_EMAIL];

                return response;
            }

            // 비밀번호 해시화
            var passwordHash = CreateHashPassword(request.Password);

            // 새로운 사용자 생성
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

            await _context.SaveChangesAsync(); // 사용자 저장

            // 약관 동의 저장
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

            await _context.SaveChangesAsync(); // 약관 저장

            await transaction.CommitAsync(); // 트랜잭션 커밋

            response.Data = user.Id;
            response.Success = true;

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); // 오류 발생 시 롤백

            response.Success = false;
            response.Data = 0;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError(string.Empty, ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var response = new ServiceResponse<LoginResponseDto>();

        try
        {
            var user = await _context.Users
                                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 1. check email
            if (user == null)
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_REGISTERED_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_REGISTERED_USER];

                return response;
            }

            // 2. check password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                response.Success = false;
                response.Data = null;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
            }

            // 3. Generate JWT token
            var token = GenerateJwtToken(user);

            response.Data = new LoginResponseDto()
            {
                id = user.Id,
                Email = user.Email,
                token = token
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = null;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError(string.Empty, ex.Message, "ip: ");

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

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}
