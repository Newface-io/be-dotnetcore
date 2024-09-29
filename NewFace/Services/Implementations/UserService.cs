﻿using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.User;
using NewFace.Models.Actor;
using NewFace.Models.Entertainment;
using NewFace.Responses;

namespace NewFace.Services;

public class UserService : IUserService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IAuthService _authService;
    private readonly IDockerFileService _fileService;

    public UserService(DataContext context, ILogService logService, IAuthService authService, IDockerFileService fileService)
    {
        _context = context;
        _logService = logService;
        _authService = authService;
        _fileService = fileService;
    }

    public async Task<ServiceResponse<bool>> DeleteUser(int userId)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            var user = await _context.Users
                                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                response.Success = false;
                response.Data = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            user.IsDeleted = true;

            await _context.SaveChangesAsync();

            response.Success = true;
            response.Data = true;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: DeleteUser", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<string>> SetUserRole(int userId, string role)
    {
        var response = new ServiceResponse<string>();

        try
        {
            var user = await _context.Users
                                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                response.Success = false;
                response.Data = string.Empty;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            if (!IsValidRole(role))
            {
                response.Success = false;
                response.Data = string.Empty;
                response.Code = MessageCode.Custom.INVALID_ROLE.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_ROLE]; ;

                return response;
            }

            var userRole = await _context.UserRole
                    .FirstOrDefaultAsync(u => u.UserId == user.Id && u.Role == role);

            if (userRole != null)
            {
                response.Success = false;
                response.Data = string.Empty;
                response.Code = MessageCode.Custom.REGISTERED_ROLE.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.REGISTERED_ROLE]; ;

                return response;
            }

            var addUserRole = new NewFace.Models.UserRole
            {
                UserId = userId,
                Role = role,
                CreatedDate = DateTime.UtcNow,
            };

            if (!_context.UserRole.Local.Any(u => u.UserId == userId && u.Role == role))
            {
                _context.UserRole.Add(addUserRole);
            }

            int roleSpecificId = 0;

            // Handle different role types
            switch (role)
            {
                case UserRole.Actor:

                    var existingActor = await _context.Actors.FirstOrDefaultAsync(a => a.UserId == userId);
                    if (existingActor == null)
                    {
                        var newActor = new Actor
                        {
                            UserId = userId
                        };

                        _context.Actors.Add(newActor);

                        await _context.SaveChangesAsync();

                        roleSpecificId = newActor.Id;
                    } else
                    {
                        roleSpecificId = existingActor.Id;
                    }

                    break;
                case UserRole.Entertainment:
                    var existingEnter = await _context.Entertainments.FirstOrDefaultAsync(a => a.UserId == userId);
                    if (existingEnter == null)
                    {
                        var newEnter = new Entertainment
                        {
                            UserId = userId
                        };

                        _context.Entertainments.Add(newEnter);

                        await _context.SaveChangesAsync();

                        roleSpecificId = newEnter.Id;
                    } else
                    {
                        roleSpecificId = existingEnter.Id;
                    }
                    break;
                default: break;
            }

            await _context.SaveChangesAsync();

            var updatedUser = await _context.Users
                                    .Include(u => u.UserRoles)
                                    .FirstOrDefaultAsync(u => u.Id == userId);

            if (updatedUser == null)
            {
                response.Success = false;
                response.Data = string.Empty;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            string newToken = _authService.GenerateJwtToken(updatedUser, role, roleSpecificId);

            response.Success = true;
            response.Data = newToken;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = string.Empty;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SetUserRole", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<IGetMyPageInfoResponseDto>> GetMyPageInfo(int userId, string role, int? roleSpecificId)
    {
        var response = new ServiceResponse<IGetMyPageInfoResponseDto>();

        try
        {
            switch (role)
            {
                case NewFace.Common.Constants.UserRole.Actor:

                    if (!roleSpecificId.HasValue)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                        return response;
                    }

                    var actorUser = await _context.Users
                        .Include(u => u.Actor)
                        .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == roleSpecificId.Value);

                    if (actorUser == null || actorUser.Actor == null)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                        return response;
                    }

                    response.Data = new ActorMyPageInfoDto
                    {
                        UserId = actorUser.Id,
                        ActorId = actorUser.Actor.Id,
                        Name = actorUser.Name,
                        Email = actorUser.Email,
                        Phone = actorUser.Phone,
                        ImageUrl = actorUser.ImageUrl
                    };

                    break;

                case NewFace.Common.Constants.UserRole.Entertainment:

                    if (!roleSpecificId.HasValue)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                        return response;
                    }

                    var enterUser = await _context.Users
                        .Include(u => u.EntertainmentProfessional)
                        .FirstOrDefaultAsync(u => u.Id == userId && u.EntertainmentProfessional.Id == roleSpecificId.Value);

                    if (enterUser == null || enterUser.EntertainmentProfessional == null)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                        return response;
                    }

                    response.Data = new EnterMyPageInfoDto
                    {
                        UserId = enterUser.Id,
                        EnterId = enterUser.EntertainmentProfessional.Id,
                        Name = enterUser.Name,
                        Email = enterUser.Email,
                        Phone = enterUser.Phone,
                        ImageUrl = enterUser.ImageUrl
                    };
                    break;

                default: // Common User
                    var commonUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (commonUser == null)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                        return response;
                    }

                    response.Data = new CommonMyPageInfoDto
                    {
                        UserId = commonUser.Id,
                        Name = commonUser.Name,
                        Email = commonUser.Email,
                        Phone = commonUser.Phone,
                        ImageUrl = commonUser.ImageUrl
                    };

                    break;
            }

            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetMyPageInfo", ex.Message, $"user id: {userId}, role: {role}, roleSpecificId: {roleSpecificId}");
        }

        return response;
    }

    public async Task<ServiceResponse<GetUserInfoForEditResponseDto>> GetUserInfoForEdit(int userId)
    {
        var response = new ServiceResponse<GetUserInfoForEditResponseDto>();

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var userProfileDto = new GetUserInfoForEditResponseDto
            {
                Name = user.Name,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                Phone = user.Phone,
                Email = user.Email
            };

            response.Success = true;
            response.Data = userProfileDto;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetUserProfile", ex.Message, $"user id: {userId}");
        }

        return response;
    }

    public async Task<ServiceResponse<bool>> UpdateUserInfoForEdit(int userId, UpdateUserInfoForEditResponseDto model)
    {
        var response = new ServiceResponse<bool>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            // Update user information
            user.Name = model.Name;
            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            user.Phone = model.Phone;
            user.Email = model.Email;
            user.LastUpdated = DateTime.UtcNow;

            string userImageRelativePath = Path.Combine("users", "image", userId.ToString());

            // Handle image upload
            if (model.Image != null)
            {
                string storagePath = await _fileService.UploadImageAndGetUrl(model.Image, userImageRelativePath);

                if (!string.IsNullOrEmpty(storagePath))
                {
                    user.ImageUrl = storagePath;
                }
                else
                {
                    response.Success = false;
                    response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
                    response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];
                    return response;
                }
            }
            else if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                // If no new image is provided and there's an existing image, delete it
                await _fileService.DeleteFileAndEmptyFolder(user.ImageUrl);
                user.ImageUrl = string.Empty;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: UpdateUserInfoForEdit", ex.Message, $"user id: {userId}");
        }

        return response;
    }

    private bool IsValidRole(string role)
    {
        return Common.Constants.UserRole.AllRoles.Contains(role);
    }

    public async Task<bool> HasUserRoleAsync(int userId, string role)
    {
        return await _context.UserRole
            .AnyAsync(ur => ur.UserId == userId && ur.Role == role);
    }
}