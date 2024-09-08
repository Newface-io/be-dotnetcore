using NewFace.Responses;

namespace NewFace.Services;

public interface IUserService
{
    Task<ServiceResponse<bool>> DeleteUser(int userId);
    Task<ServiceResponse<int>> SetUserRole(int userId, string role);
    Task<bool> HasUserRoleAsync(int userId, string role);
}
