using Microsoft.EntityFrameworkCore;
using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;

namespace NewFace.Services;

public class ActorService : IActorService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IFileService _fileService;
    
    public ActorService(DataContext context, ILogService logService, IFileService fileService)
    {
        _context = context;
        _logService = logService;
        _fileService = fileService;
    }

    #region 1. profile

    public async Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId, int actorId)
    {
        var response = new ServiceResponse<GetActorResponseDto>();

        try
        {
            var user = await _context.Users
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Experiences)
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Education)
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Links)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            // 1. check if user and actor data is exist
            if (user == null || user.Actor == null || user.Actor.Id != actorId)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var actorDto = new GetActorResponseDto
            {
                UserId = userId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ActorId = user.Actor.Id,
                BirthDate = user.Actor.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                Address = user.Actor.Address,
                Height = user.Actor.Height?.ToString() ?? string.Empty,
                Weight = user.Actor.Weight?.ToString() ?? string.Empty,
                Bio = user.Actor.Bio ?? string.Empty,
                Gender = user.Actor.Gender ?? string.Empty,

                Role = NewFace.Common.Constants.UserRole.Actor,

                ActorEducations = user.Actor.Education.ToList(),
                ActorExperiences = user.Actor.Experiences.ToList(),
                ActorLinks = user.Actor.Links.ToList(),
                
            };

            response.Success = true;
            response.Data = actorDto;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, "user id: " + userId);

            return response;
        }
    }

    public async Task<ServiceResponse<int>> UpdateActorProfile(int userId, int actorId, UpdateActorProfileRequestDto model)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Education)
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Links)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            // 1. check if user and actor data is exist
            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            // Update User properties
            if (existingUserWithActor.Name != model.Name || existingUserWithActor.Email != model.Email)
            {
                existingUserWithActor.Name = model.Name;
                existingUserWithActor.Email = model.Email;
                existingUserWithActor.LastUpdated = DateTime.UtcNow;
                _context.Users.Update(existingUserWithActor);
            }

            // Update Actor properties
            var existingActor = existingUserWithActor.Actor;
            if (existingActor.BirthDate != model.BirthDate ||
                existingActor.Gender != model.Gender ||
                existingActor.Height != model.Height ||
                existingActor.Weight != model.Weight ||
                existingActor.Bio != model.Bio)
            {
                existingActor.BirthDate = model.BirthDate;
                existingActor.Gender = model.Gender;
                existingActor.Height = model.Height;
                existingActor.Weight = model.Weight;
                existingActor.Bio = model.Bio;
                existingActor.LastUpdated = DateTime.UtcNow;
                _context.Actors.Update(existingActor);
            }

            // Update Education
            _context.ActorEducations.RemoveRange(existingActor.Education);
            if (model.Education != null)
            {
                existingActor.Education = model.Education.Select(e => new ActorEducation
                {
                    ActorId = existingUserWithActor.Actor.Id,
                    EducationType = e.EducationType,
                    GraduationStatus = e.GraduationStatus,
                    School = e.School,
                    Major = e.Major
                }).ToList();
            }

            // Update Links
            _context.ActorLinks.RemoveRange(existingActor.Links);
            if (model.Links != null)
            {
                existingActor.Links = model.Links.Select(l => new ActorLink
                {
                    ActorId = existingUserWithActor.Actor.Id,
                    Category = l.Category,
                    Url = l.Url,
                }).ToList();
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = existingActor.Id;
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: UpdateActorProfile", ex.Message, $"user id: {userId}");

            return response;
        }
    }
    #endregion

    #region 2. demo star

    public async Task<ServiceResponse<ActorDemoStarListDto>> GetActorDemoStar(int userId, int actorId)
    {
        var response = new ServiceResponse<ActorDemoStarListDto>();

        try
        {
            var existingUserWithActor = await _context.Users
                                                .Include(u => u.Actor)
                                                    .ThenInclude(a => a.DemoStars)
                                                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            // 1. Check if user and actor data exist
            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            // 3. Prepare the DTO
            var actorDemoStarListDto = new ActorDemoStarListDto
            {
                ActorId = existingUserWithActor.Actor.Id,
                DemoStars = existingUserWithActor.Actor.DemoStars.Select(ds => new DemoStarDto
                {
                    Id = ds.Id,
                    Title = ds.Title,
                    Category = ds.Category,
                    Url = ds.Url
                }).ToList()
            };

            response.Success = true;
            response.Data = actorDemoStarListDto;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {userId} , actor id: {actorId}");

            return response;
        }
    }

    public async Task<ServiceResponse<int>> AddActorDemoStar(int userId, int actorId, AddActorDemoStarDto model)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            // 1. check if user and actor data is exist
            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            // 2. Create and add new ActorDemoStar
            var newActorDemoStar = new ActorDemoStar
            {
                ActorId = existingUserWithActor.Actor.Id,
                Title = model.Title,
                Category = model.Category,
                Url = model.Url
            };

            _context.ActorDemoStars.Add(newActorDemoStar);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = newActorDemoStar.Id;
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: AddActorDemoStar", ex.Message, $"user id: {userId} actor id: {actorId}");

            return response;
        }
    }


    public async Task<ServiceResponse<int>> UpdateActorDemoStar(int userId, int actorId, UpdateActorDemoStarDto model)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                    .ThenInclude(a => a.DemoStars)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            // 1. check if user and actor data is exist
            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var existingDemoStar = await _context.ActorDemoStars
                    .FirstOrDefaultAsync(ds => ds.Id == model.Id);

            // 2. check if DemoStar exists
            if (existingDemoStar == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_DEMOSTAR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DEMOSTAR];
                return response;
            }

            // 3. update demo star
            existingDemoStar.Title = model.Title;
            existingDemoStar.Category = model.Category;
            existingDemoStar.Url = model.Url;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = existingDemoStar.Id;
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: UpdateActorDemoStar", ex.Message, $"user id: {userId} , actor demo star id: {model.Id}");

            return response;
        }
    }

    public async Task<ServiceResponse<int>> DeleteActorDemoStar(int userId, int actorId, int demoStarId)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                    .ThenInclude(a => a.DemoStars)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            // 1. check if user and actor data is exist
            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            // 2. check actor role
            //if (!await _userService.HasUserRoleAsync(existingUserWithActor.Id, NewFace.Common.Constants.UserRole.Actor))
            //{
            //    response.Success = false;
            //    response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
            //    response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
            //    return response;
            //}

            var existingDemoStar = await _context.ActorDemoStars
                    .FirstOrDefaultAsync(ds => ds.Id == demoStarId);

            // 2. check if DemoStar exists
            if (existingDemoStar == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_DEMOSTAR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DEMOSTAR];
                return response;
            }

            // 3. Remove the DemoStar
            _context.ActorDemoStars.Remove(existingDemoStar);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = demoStarId;
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: DeleteActorDemoStar", ex.Message, $"user id: {userId} , actor demo star id: {demoStarId}");

            return response;
        }
    }

    #endregion

    #region 3. image
    public async Task<ServiceResponse<GetActorImagesResponseDto>> GetActorImages(int userId, int actorId)
    {
        var response = new ServiceResponse<GetActorImagesResponseDto>();

        try
        {
            var existingUserWithActor = await _context.Users
                    .Include(u => u.Actor)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var actorImages = await _context.ActorImages
                .Where(ai => ai.ActorId == actorId && ai.IsDeleted == false)
                .GroupBy(ai => ai.GroupId)
                .Select(g => new
                {
                    GroupId = g.Key,
                    FirstImage = g.OrderBy(ai => ai.GroupOrder).First(),    // group의 첫번째 이미지
                    ImageCount = g.Count(),                                 // group에서 이미지 개수
                    ActorOrder = g.First().ActorOrder,                      // group의 첫번째 이미지 Order (어차피 groupId가 같으면 ActorOrder는 동일)
                    IsMainImage = g.Any(ai => ai.IsMainImage)
                })
                .OrderBy(g => g.ActorOrder)
                .ToListAsync();

            var images = actorImages.Select(g => new GetActorImages
            {
                ImageId = g.FirstImage.Id,
                PublicUrl = g.FirstImage.PublicUrl,
                FileName = g.FirstImage.FileName,
                GroupId = g.GroupId,
                GroupOrder = g.FirstImage.GroupOrder,
                ImageCount = g.ImageCount,
                IsMainImage = g.IsMainImage
            }).ToList();

            var result = new GetActorImagesResponseDto
            {
                ActorId = actorId,
                Images = images
            };

            response.Success = true;
            response.Data = result;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetActorImages", ex.Message, $"user id: {userId} , actor id: {actorId}");

            return response;
        }

        return response;
    }

    public async Task<ServiceResponse<GetActorImagesByGroupResponseDto>> GetActorImagesByGroup(int userId, int actorId, int groupId)
    {
        var response = new ServiceResponse<GetActorImagesByGroupResponseDto>();

        try
        {
            var existingUserWithActor = await _context.Users
                    .Include(u => u.Actor)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var groupImages = await _context.ActorImages
                .Where(ai => ai.ActorId == actorId && ai.GroupId == groupId && ai.IsDeleted == false)
                .OrderBy(ai => ai.GroupOrder)
                .Select(ai => new ActorImageDto
                {
                    ImageId = ai.Id,
                    PublicUrl = ai.PublicUrl,
                    FileName = ai.FileName,
                    GroupOrder = ai.GroupOrder
                })
                .ToListAsync();

            var result = new GetActorImagesByGroupResponseDto
            {
                ActorId = actorId,
                GroupId = groupId,
                Images = groupImages
            };

            response.Success = true;
            response.Data = result;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetActorImagesByGroup", ex.Message, $"user id: {userId}, actor id: {actorId}, group id: {groupId}");
        }

        return response;
    }

    public async Task<ServiceResponse<bool>> UploadActorImages(int userId, int actorId, UploadActorImagesRequestDto model)
    {
        var response = new ServiceResponse<bool>();

        // 1. 모든 파일의 형식을 먼저 검사
        foreach (var file in model.Images)
        {
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!_fileService.IsAllowedImageFileType(fileExtension))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_FILE_TYPE.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_FILE_TYPE];
                return response;
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var existingImages = await _context.ActorImages
                                        .AnyAsync(ai => ai.ActorId == actorId && !ai.IsDeleted);

            var groupOrder = 1;
            var uploadedImages = new List<ActorImage>();
            var nextActorOrder = await GetNextActorOrderAsync(actorId);
            var newGroupId = await GetNextGroupIdAsync(actorId);

            foreach (var file in model.Images)
            {
                if (file.Length > 0)
                {
                    string relativePath = Path.Combine("actors", "image", actorId.ToString());
                    string storagePath = await _fileService.UploadImageAndGetUrl(file, relativePath);

                    if (storagePath.Equals(string.Empty))
                    {
                        await transaction.RollbackAsync();
                        _logService.LogError("ERROR: UploadActorImages", "storage upload error", $"Error uploading images for actorId: {actorId}");
                        response.Success = false;
                        response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
                        response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];
                        response.Data = false;
                    }

                    var actorImage = new ActorImage
                    {
                        ActorId = actorId,
                        GroupId = newGroupId,
                        GroupOrder = groupOrder,
                        ActorOrder = nextActorOrder,
                        StoragePath = storagePath,
                        PublicUrl = string.Empty, // S3 올라가면 처리
                        FileName = Path.GetFileName(storagePath),
                        FileType = Path.GetExtension(file.FileName).TrimStart('.'),
                        FileSize = file.Length,
                        IsMainImage = !existingImages && groupOrder == 1 ? true : false,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    uploadedImages.Add(actorImage);

                    groupOrder++;
                }
            }

            await _context.ActorImages.AddRangeAsync(uploadedImages);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            response.Data = true;
            response.Success = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logService.LogError("EXCEPTION: UploadActorImages", ex.Message, $"Error uploading images for actorId: {actorId}");
            response.Success = false;
            response.Code = MessageCode.Custom.FAILED_FILE_UPLOAD.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.FAILED_FILE_UPLOAD];
            response.Data = false;
        }

        return response;
    }

    public async Task<ServiceResponse<bool>> DeleteActorImages(int userId, int actorId, List<int> groupIds)
    {
        var response = new ServiceResponse<bool>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingUserWithActor = await _context.Users
                    .Include(u => u.Actor)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            foreach (var groupId in groupIds)
            {
                var imagesToDelete = await _context.ActorImages
                    .Where(ai => ai.ActorId == actorId && ai.GroupId == groupId && !ai.IsDeleted)
                    .ToListAsync();

                if (!imagesToDelete.Any())
                {
                    await transaction.RollbackAsync();
                    response.Success = false;
                    response.Code = MessageCode.Custom.INVALID_FILE.ToString();
                    response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_FILE];
                    return response;
                }

                foreach (var image in imagesToDelete)
                {
                    image.IsDeleted = true;
                    image.IsMainImage = false;
                    image.DeletedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Success = true;
            response.Data = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: DeleteActorImages", ex.Message, $"user id: {userId}, actor id: {actorId}");
        }

        return response;
    }

    public async Task<ServiceResponse<int>> SetActorMainImage(int userId, int actorId, int groupId)
    {
        var response = new ServiceResponse<int>();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 현재 배우의 모든 이미지에서 IsMain을 false로 설정
                var currentActorImages = await _context.ActorImages
                    .Where(ai => ai.ActorId == actorId && !ai.IsDeleted)
                    .ToListAsync();

                foreach (var image in currentActorImages)
                {
                    image.IsMainImage = false;
                }

                // 2. 선택된 그룹의 첫 번째 이미지를 찾아 IsMain을 true로 설정
                var newMainImage = await _context.ActorImages
                    .Where(ai => ai.ActorId == actorId && ai.GroupId == groupId && !ai.IsDeleted)
                    .OrderBy(ai => ai.GroupOrder)
                    .FirstOrDefaultAsync();

                if (newMainImage == null)
                {
                    response.Success = false;
                    response.Code = MessageCode.Custom.NOT_FOUND_DATA.ToString();
                    response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_DATA];
                    return response;
                }

                newMainImage.IsMainImage = true;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                response.Success = true;
                response.Data = groupId;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

                _logService.LogError("EXCEPTION: SetActorMainImage1", ex.Message, $"user id: {userId}, actor id: {actorId}, group id: {groupId}");
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: SetActorMainImage2", ex.Message, $"user id: {userId}, actor id: {actorId}, group id: {groupId}");
        }

        return response;
    }

    private async Task<int> GetNextGroupIdAsync(int actorId)
    {
        var maxGroupId = await _context.ActorImages
            .Where(ai => ai.ActorId == actorId)
            .MaxAsync(ai => (int?)ai.GroupId) ?? 0;
        return maxGroupId + 1;
    }

    private async Task<int> GetNextActorOrderAsync(int actorId)
    {
        var maxOrder = await _context.ActorImages
            .Where(ai => ai.ActorId == actorId)
            .MaxAsync(ai => (int?)ai.ActorOrder) ?? 0;
        return maxOrder + 1;
    }

    #endregion

    #region 4. experience

    public async Task<ServiceResponse<GetActorExperiencesResponseDto>> GetActorExperiences(int userId, int actorId)
    {
        var response = new ServiceResponse<GetActorExperiencesResponseDto>();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            var experiences = await _context.ActorExperiences
                .Where(ae => ae.ActorId == actorId)
                .Select(ae => new ActorExperiences
                {
                    ExperienceId = ae.Id,
                    Category = ae.Category,
                    WorkTitle = ae.WorkTitle,
                    Role = ae.Role,
                    RoleName = ae.RoleName,
                    StartDate = ae.StartDate,
                    EndDate = ae.EndDate,
                    CreatedDate = ae.CreatedDate,
                    LastUpdated = ae.LastUpdated
                })
                .ToListAsync();

            response.Success = true;
            response.Data = new GetActorExperiencesResponseDto
            {
                ActorId = actorId,
                ActorExperiences = experiences
            };
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: GetActorExperiences", ex.Message, $"user id: {userId}, actor id: {actorId}");
        }

        return response;
    }

    public async Task<ServiceResponse<bool>> UpdateActorExperiences(int userId, int actorId, UpdateActorExperiencesRequestDto model)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            var existingUserWithActor = await _context.Users
                .Include(u => u.Actor)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Actor.Id == actorId);

            if (existingUserWithActor == null || existingUserWithActor.Actor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Get existing experiences
                var existingExperiences = await _context.ActorExperiences
                    .Where(ae => ae.ActorId == actorId)
                    .ToListAsync();

                // 2. Process each experience in the request
                foreach (var experienceDto in model.UpdateActorExperiences)
                {
                    if (experienceDto.ExperienceId == 0)
                    {
                        // Create new experience
                        var newExperience = new ActorExperience
                        {
                            ActorId = actorId,
                            Category = experienceDto.Category,
                            WorkTitle = experienceDto.WorkTitle,
                            Role = experienceDto.Role,
                            RoleName = experienceDto.RoleName,
                            StartDate = experienceDto.StartDate,
                            EndDate = experienceDto.EndDate
                        };
                        await _context.ActorExperiences.AddAsync(newExperience);
                    }
                    else
                    {
                        // Update existing experience
                        var existingExperience = existingExperiences.FirstOrDefault(e => e.Id == experienceDto.ExperienceId);
                        if (existingExperience != null)
                        {
                            existingExperience.Category = experienceDto.Category;
                            existingExperience.WorkTitle = experienceDto.WorkTitle;
                            existingExperience.Role = experienceDto.Role;
                            existingExperience.RoleName = experienceDto.RoleName;
                            existingExperience.StartDate = experienceDto.StartDate;
                            existingExperience.EndDate = experienceDto.EndDate;
                        }
                    }
                }

                // 3. Remove experiences not in the request
                var experienceIdsToKeep = model.UpdateActorExperiences
                    .Where(e => e.ExperienceId != 0)
                    .Select(e => e.ExperienceId)
                    .ToList();
                var experiencesToRemove = existingExperiences
                    .Where(e => !experienceIdsToKeep.Contains(e.Id))
                    .ToList();
                _context.ActorExperiences.RemoveRange(experiencesToRemove);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Success = true;
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logService.LogError("EXCEPTION: UpdateActorExperiences1", ex.Message, $"user id: {userId}, actor id: {actorId}");

                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION: UpdateActorExperiences2", ex.Message, $"user id: {userId}, actor id: {actorId}");
        }

        return response;
    }

    #endregion

}
