using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.User;
using NewFace.Models.Actor;
using NewFace.Models.Entertainment;
using NewFace.Models.User;
using NewFace.Responses;

namespace NewFace.Services;

public class UserService : IUserService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IAuthService _authService;
    private readonly IFileService _fileService;
    private readonly IMemoryManagementService _memoryManagementService;

    public UserService(DataContext context, ILogService logService, IAuthService authService, IFileService fileService, IMemoryManagementService memoryManagementService)
    {
        _context = context;
        _logService = logService;
        _authService = authService;
        _fileService = fileService;
        _memoryManagementService = memoryManagementService;
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

            //user.IsDeleted = true;
            _context.Users.Remove(user);

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

            var addUserRole = new NewFace.Models.User.UserRole
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
                case NewFace.Common.Constants.USER_ROLE.ACTOR:

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
                case NewFace.Common.Constants.USER_ROLE.ENTER:
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
                case NewFace.Common.Constants.USER_ROLE.ACTOR:

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
                        ImageUrl = actorUser.PublicUrl
                    };

                    break;

                case NewFace.Common.Constants.USER_ROLE.ENTER:

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
                        ImageUrl = enterUser.PublicUrl,
                        CompanyName = enterUser.EntertainmentProfessional.CompanyName
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
                        ImageUrl = commonUser.PublicUrl
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

    public async Task<ServiceResponse<IGetUserInfoForEditResponseDto>> GetUserInfoForEdit(int userId, string role, int? roleSpecificId)
    {
        var response = new ServiceResponse<IGetUserInfoForEditResponseDto>();

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new ServiceResponse<IGetUserInfoForEditResponseDto>
                {
                    Success = false,
                    Code = MessageCode.Custom.NOT_FOUND_USER.ToString(),
                    Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER]
                };
            }

            var entertainment = (role == NewFace.Common.Constants.USER_ROLE.ENTER && roleSpecificId.HasValue)
                ? await _context.Entertainments.FirstOrDefaultAsync(e => e.Id == roleSpecificId.Value)
                : null;

            if (role == NewFace.Common.Constants.USER_ROLE.ENTER && entertainment == null)
            {
                return new ServiceResponse<IGetUserInfoForEditResponseDto>
                {
                    Success = false,
                    Code = MessageCode.Custom.NOT_FOUND_USER.ToString(),
                    Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER]
                };
            }

            IGetUserInfoForEditResponseDto userProfileDto = role switch
            {
                NewFace.Common.Constants.USER_ROLE.ACTOR => new ActorUserInfoForEditDto
                {
                    Name = user.Name,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
                },
                NewFace.Common.Constants.USER_ROLE.ENTER when entertainment != null => new EnterUserInfoForEditDto
                {
                    Name = user.Name,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email,
                    CompanyName = entertainment.CompanyName,
                    CeoName = entertainment.CeoName,
                    CompanyAddress = entertainment.CompanyAddress,
                    BusinessCardImagePublicUrl = string.Empty,
                    BusinessLicenseImagePublicUrl = string.Empty,
                },
                _ => new CommonUserInfoForEditDto
                {
                    Name = user.Name,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    Email = user.Email
                }
            };

            response.Success = true;
            response.Data = userProfileDto;

        }
        catch (Exception ex)
        {
            response = new ServiceResponse<IGetUserInfoForEditResponseDto>
            {
                Success = false,
                Code = MessageCode.Custom.UNKNOWN_ERROR.ToString(),
                Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR]
            };

            _logService.LogError("EXCEPTION: GetUserProfile", $"{ex.Message}\n{ex.StackTrace}", $"user id: {userId}");
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
            if (model.isUpdatedImage)
            {
                var folderPath = $"User/Image/{userId}";

                if (model.Image != null)
                {
                    var uploadResult = await _fileService.UploadFile(model.Image, folderPath);

                    if (!uploadResult.Success)
                    {
                        await transaction.RollbackAsync();
                        _logService.LogError("ERROR: UpdateUserInfoForEdit", "storage upload error", $"Error uploading images for userId: {userId}");
                        response.Success = false;
                        response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];
                        response.Data = false;

                        return response;
                    }

                    await _fileService.DeleteFile(user.StoragePath);

                    var (storagePath, publicUrl) = (uploadResult.Data.S3Path, uploadResult.Data.CloudFrontUrl);

                    user.StoragePath = storagePath; 
                    user.PublicUrl = publicUrl;
                } else
                {
                    var deleteResult = await _fileService.DeleteFile(user.StoragePath);

                    if (!deleteResult.Success)
                    {
                        await transaction.RollbackAsync();
                        _logService.LogError("ERROR: UpdateUserInfoForEdit", "storage delete error", $"Error delete images for userId: {userId}");
                        response.Success = false;
                        response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];
                        response.Data = false;

                        return response;
                    }

                    user.StoragePath = string.Empty;
                    user.PublicUrl = string.Empty;
                }
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
        return Common.Constants.USER_ROLE.AllRoles.Contains(role);
    }

    public async Task<bool> HasUserRoleAsync(int userId, string role)
    {
        return await _context.UserRole
            .AnyAsync(ur => ur.UserId == userId && ur.Role == role);
    }

    /// <param name="itemId"></param> DemoStar ID | Actor ID
    /// <param name="likeType"></param> DemoStar | Portpolio
    public async Task<ServiceResponse<bool>> ToggleLike(int userId, int itemId, string likeType)
    {
        var response = new ServiceResponse<bool>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

            switch (likeType)
            {
                // itemId: demostarId
                case LikeType.DemoStar:

                    var demoStar = await _context.ActorDemoStars.FindAsync(itemId); 

                    if (user == null || demoStar == null)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.INVALID_DATA.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DATA];
                        return response;
                    }

                    var existingLikeForDemoStar = await _context.UserLikes
                        .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.ItemId == itemId && ul.ItemType == likeType);

                    if (existingLikeForDemoStar != null)
                    {
                        // Remove like
                        _context.UserLikes.Remove(existingLikeForDemoStar);
                        DecrementLikeCount(user, demoStar);
                    }
                    else
                    {
                        // Add like
                        var newLike = new UserLike
                        {
                            UserId = userId,
                            ItemId = itemId,
                            ItemType = LikeType.DemoStar,
                            CreatedDateTime = DateTime.UtcNow
                        };

                        _context.UserLikes.Add(newLike);
                        IncrementLikeCount(user, demoStar);
                    }
                    break;

                // itemId: actorId
                case LikeType.Portfolio:

                    var actor = await _context.Actors.FindAsync(itemId);

                    if (user == null || actor == null)
                    {
                        response.Success = false;
                        response.Code = MessageCode.Custom.INVALID_DATA.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DATA];
                        return response;
                    }

                    var existingLikeForPortfolio = await _context.UserLikes
                        .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.ItemId == itemId && ul.ItemType == likeType);

                    if (existingLikeForPortfolio != null)
                    {
                        // Remove like
                        _context.UserLikes.Remove(existingLikeForPortfolio);
                        DecrementLikeCount(user, actor);
                    }
                    else
                    {
                        // Add like
                        var newLike = new UserLike
                        {
                            UserId = userId,
                            ItemId = itemId,
                            ItemType = LikeType.Portfolio,
                            CreatedDateTime = DateTime.UtcNow
                        };

                        _context.UserLikes.Add(newLike);
                        IncrementLikeCount(user, actor);
                    }
                    break;
                default:
                    break;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _memoryManagementService.InvalidateCache("AllActorPortfolios");

            response.Data = true;
            response.Success = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Data = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("ToggleLike", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }

    private void IncrementLikeCount(User user, ActorDemoStar demoStar)
    {
        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();

        if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.COMMON))
        {
            demoStar.LikesFromCommons++;
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ACTOR))
        {
            demoStar.LikesFromActors++;
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ENTER))
        {
            demoStar.LikesFromEnters++;
        }
    }

    private void IncrementLikeCount(User user, Actor actor)
    {
        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();

        if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.COMMON))
        {
            actor.LikesFromCommons++;
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ACTOR))
        {
            actor.LikesFromActors++;
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ENTER))
        {
            actor.LikesFromEnters++;
        }
    }


    private void DecrementLikeCount(User user, ActorDemoStar demoStar)
    {
        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();

        if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.COMMON))
        {
            demoStar.LikesFromCommons = Math.Max(0, demoStar.LikesFromCommons - 1);
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ACTOR))
        {
            demoStar.LikesFromActors = Math.Max(0, demoStar.LikesFromActors - 1);
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ENTER))
        {
            demoStar.LikesFromEnters = Math.Max(0, demoStar.LikesFromEnters - 1);
        }
    }

    private void DecrementLikeCount(User user, Actor actor)
    {
        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();

        if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.COMMON))
        {
            actor.LikesFromCommons = Math.Max(0, actor.LikesFromCommons - 1);
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ACTOR))
        {
            actor.LikesFromActors = Math.Max(0, actor.LikesFromActors - 1);
        }
        else if (userRoles.Contains(NewFace.Common.Constants.USER_ROLE.ENTER))
        {
            actor.LikesFromEnters = Math.Max(0, actor.LikesFromEnters - 1);
        }
    }
}
