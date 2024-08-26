using NewFace.Responses;

namespace NewFace.Services;

public interface IUserService
{
    Task<ServiceResponse<bool>> DeleteUser(int userId);
}
