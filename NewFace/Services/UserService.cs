using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.Responses;

namespace NewFace.Services;

public class UserService : IUserService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;

    public UserService(DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
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

            user.isDeleted = true;

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

            _logService.LogError("EXCEPTION", ex.Message, "ip: ");

            return response;
        }
    }

    public async Task<ServiceResponse<int>> SetUserRole(int userId, string role)
    {
        var response = new ServiceResponse<int>();

        try
        {
            var user = await _context.Users
                                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            if (!IsValidRole(role))
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.INVALID_ROLE.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_ROLE]; ;

                return response;
            }

            var userRole = await _context.UserRole
                    .FirstOrDefaultAsync(u => u.UserId == user.Id && u.Role == role);

            if (userRole != null)
            {
                response.Success = false;
                response.Data = 0;
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

            await _context.SaveChangesAsync();

            response.Success = true;
            response.Data = userId;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Data = 0;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, "ip: ");

            return response;
        }
    }

    private bool IsValidRole(string role)
    {
        return Common.Constants.UserRole.AllRoles.Contains(role);
    }
}
