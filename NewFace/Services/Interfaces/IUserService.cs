using NewFace.DTOs.User;
using NewFace.Responses;

namespace NewFace.Services;

public interface IUserService
{
    Task<ServiceResponse<bool>> DeleteUser(int userId);
    Task<ServiceResponse<string>> SetUserRole(int userId, string role);
    Task<ServiceResponse<IGetMyPageInfoResponseDto>> GetMyPageInfo(int userId, string role, int? roleSpecificId);
    Task<ServiceResponse<IGetUserInfoForEditResponseDto>> GetUserInfoForEdit(int userId, string role, int? roleSpecificId);
    Task<ServiceResponse<bool>> UpdateUserInfoForEdit(int userId, UpdateUserInfoForEditResponseDto model);
    Task<bool> HasUserRoleAsync(int userId, string role);
    Task<ServiceResponse<bool>> ToggleLike(int userId, int itemId, string likeType);
}
