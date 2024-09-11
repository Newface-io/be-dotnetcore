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
    private readonly IUserService _userService;

    public ActorService(DataContext context, ILogService logService, IUserService userService)
    {
        _context = context;
        _logService = logService;
        _userService = userService;
    }

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

            // 2. check actor role
            if (!await _userService.HasUserRoleAsync(user.Id, NewFace.Common.Constants.UserRole.Actor))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
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

            // 2. check actor role
            if (!await _userService.HasUserRoleAsync(existingUserWithActor.Id, NewFace.Common.Constants.UserRole.Actor))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
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

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {userId}");

            return response;
        }
    }

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

            // 2. Check actor role
            if (!await _userService.HasUserRoleAsync(existingUserWithActor.Id, NewFace.Common.Constants.UserRole.Actor))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
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

            // 2. check actor role
            if (!await _userService.HasUserRoleAsync(existingUserWithActor.Id, NewFace.Common.Constants.UserRole.Actor))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
                return response;
            }

            // 3. Create and add new ActorDemoStar
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

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {userId} actor id: {actorId}");

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

            // 2. check actor role
            if (!await _userService.HasUserRoleAsync(existingUserWithActor.Id, NewFace.Common.Constants.UserRole.Actor))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
                return response;
            }

            var existingDemoStar = await _context.ActorDemoStars
                    .FirstOrDefaultAsync(ds => ds.Id == model.Id);

            // 3. check if DemoStar exists
            if (existingDemoStar == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_DEMOSTAR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DEMOSTAR];
                return response;
            }

            // 4. update demo star
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

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {userId} , actor demo star id: {model.Id}");

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
            if (!await _userService.HasUserRoleAsync(existingUserWithActor.Id, NewFace.Common.Constants.UserRole.Actor))
            {
                response.Success = false;
                response.Code = MessageCode.Custom.USER_NOT_ACTOR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.USER_NOT_ACTOR];
                return response;
            }

            var existingDemoStar = await _context.ActorDemoStars
                    .FirstOrDefaultAsync(ds => ds.Id == demoStarId);

            // 3. check if DemoStar exists
            if (existingDemoStar == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_DEMOSTAR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DEMOSTAR];
                return response;
            }

            // 4. Remove the DemoStar
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

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {userId} , actor demo star id: {demoStarId}");

            return response;
        }
    }

}
