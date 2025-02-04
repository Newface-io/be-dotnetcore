using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Actor;
using NewFace.Responses;
using NewFace.Services.Interfaces;

namespace NewFace.Services.Implementations;

public class EntertainmentService : IEntertainmentService
{

    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;

    public EntertainmentService(DataContext context, ILogService logService, IFileService fileService)
    {
        _context = context;
        _logService = logService;
        _fileService = fileService;
    }

    public string CompanyType { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string CeoName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public string ContactDepartment { get; set; } = string.Empty;
    public string ContactPosition { get; set; } = string.Empty;

    public async Task<ServiceResponse<int>> UpdateEntertainmentProfile(UpdateEntertainmentProfileRequestDto model)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var enterUser = await _context.Entertainments.Where(e => e.UserId == model.UserId).FirstOrDefaultAsync();

            if (enterUser == null || !await _context.Users.AnyAsync(u => u.Id == model.UserId))
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            enterUser.CompanyType = model.CompanyType;
            enterUser.CompanyName = model.CompanyName;
            enterUser.CeoName = model.CeoName;
            enterUser.CompanyAddress = model.CompanyAddress;
            enterUser.ContactName = model.ContactName;
            enterUser.ContactPhone = model.ContactPhone;
            enterUser.ContactEmail = model.ContactEmail;
            enterUser.ContactDepartment = model.ContactDepartment;
            enterUser.ContactPosition = model.ContactPosition;

            if (model.isUpdatedImage)
            {
                var folderPath = $"User/Image/Enter/{model.UserId}";

                if (model.BusinessLicenseImage != null)
                {
                    var uploadResult = await _fileService.UploadFile(model.BusinessLicenseImage, folderPath);

                    if (!uploadResult.Success)
                    {
                        await transaction.RollbackAsync();

                        _logService.LogError("ERROR: UpdateEntertainmentProfile", "storage upload error", $"Error uploading images for userId: {model.UserId}");
                        response.Success = false;
                        response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];

                        return response;
                    }

                    var (storagePath, publicUrl) = (uploadResult.Data.S3Path, uploadResult.Data.CloudFrontUrl);

                    enterUser.BusinessLicenseImagePublicUrl = publicUrl;
                }

                if (model.BusinessCardImage != null)
                {
                    var uploadResult = await _fileService.UploadFile(model.BusinessCardImage, folderPath);

                    if (!uploadResult.Success)
                    {
                        await transaction.RollbackAsync();

                        _logService.LogError("ERROR: UpdateEntertainmentProfile", "storage upload error", $"Error uploading images for userId: {model.UserId}");
                        response.Success = false;
                        response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];

                        return response;
                    }

                    var (storagePath, publicUrl) = (uploadResult.Data.S3Path, uploadResult.Data.CloudFrontUrl);

                    enterUser.BusinessCardImagePublicUrl = publicUrl;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = enterUser.Id;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: UpdateEntertainmentProfile", ex.Message, "user id: " + model.UserId);

            return response;
        }
    }
}
