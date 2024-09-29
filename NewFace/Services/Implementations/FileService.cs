using Amazon.CloudFront.Model;
using Amazon.CloudFront;
using Amazon.S3.Transfer;
using Amazon.S3;
using NewFace.Responses;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;

namespace NewFace.Services;

public class FileService : IFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonCloudFront _cloudFrontClient;
    private readonly ILogService _logService;
    private readonly string _bucketName;
    private readonly string _cloudFrontDomain;
    //private readonly string _cloudFrontDistributionId; // 나중에 cache 제거를 위해 필요

    public FileService(ILogService logService, IConfiguration configuration)
    {
        _s3Client = new AmazonS3Client(RegionEndpoint.APNortheast2);
        _cloudFrontClient = new AmazonCloudFrontClient(RegionEndpoint.APNortheast2);
        _logService = logService;
        _bucketName = configuration["AWS:S3BucketName"] ?? string.Empty;
        _cloudFrontDomain = configuration["AWS:CloudFrontDomain"] ?? string.Empty;
        //_cloudFrontDistributionId = configuration["AWS:CloudFrontDistributionId"] ?? string.Empty;
    }

    public bool IsAllowedImageFileType(string fileExtension)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        return allowedExtensions.Contains(fileExtension.ToLower());
    }

    public async Task<ServiceResponse<(string S3Path, string CloudFrontUrl)>> UploadFile(IFormFile file, string folderPath)
    {
        var response = new ServiceResponse<(string S3Path, string CloudFrontUrl)>();
        try
        {
            var fileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";

            var s3Key = $"{folderPath.TrimStart('/')}/{fileName}";

            using (var fileStream = file.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = s3Key,
                    BucketName = _bucketName,
                    Metadata =
                {
                    ["Content-Type"] = file.ContentType
                }
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            var s3Path = $"s3://{_bucketName}/{s3Key}";
            var cloudFrontUrl = $"{_cloudFrontDomain}/{s3Key}";

            response.Data = (s3Path, cloudFrontUrl);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "File upload failed";
            _logService.LogError("FileUpload", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }

    public async Task<ServiceResponse<bool>> MoveFileToDeletedFolder(string fileName)
    {
        var response = new ServiceResponse<bool>();
        try
        {
            var sourceKey = fileName;
            var destinationKey = $"deleted/{fileName}";

            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                SourceKey = sourceKey,
                DestinationBucket = _bucketName,
                DestinationKey = destinationKey
            };

            await _s3Client.CopyObjectAsync(copyRequest);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = sourceKey
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);

            response.Data = true;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "Failed to move file to deleted folder";
            _logService.LogError("MoveFileToDeletedFolder", ex.Message, ex.StackTrace ?? string.Empty);
        }
        return response;
    }
}