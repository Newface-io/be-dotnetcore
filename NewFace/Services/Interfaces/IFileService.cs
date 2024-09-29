using NewFace.Responses;

namespace NewFace.Services;

public interface IFileService
{
    bool IsAllowedImageFileType(string fileExtension);
    Task<ServiceResponse<(string S3Path, string CloudFrontUrl)>> UploadFile(IFormFile file, string folderPath);
    Task<ServiceResponse<bool>> MoveFileToDeletedFolder(string fileName);
}
