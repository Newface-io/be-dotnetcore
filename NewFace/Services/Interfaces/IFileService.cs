using NewFace.DTOs.Auth;
using NewFace.DTOs.File;
using NewFace.Responses;

namespace NewFace.Services;

public interface IFileService
{
    Task<List<UserFileInfo>> GetUserFilesByType(int userId, string type);
    Task<bool> UploadFiles(FileUploadRequest request);
}
