using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.Models.Actor;
using NewFace.Models.Entertainment;
using NewFace.Responses;

namespace NewFace.Services;

public class UserService : IUserService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IAuthService _authService;

    public UserService(DataContext context, ILogService logService, IAuthService authService)
    {
        _context = context;
        _logService = logService;
        _authService = authService;
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
