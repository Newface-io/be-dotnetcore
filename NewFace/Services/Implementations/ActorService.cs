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

    public async Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId)
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
                .FirstOrDefaultAsync(u => u.Id == userId);

            // 1. check if user and actor data is exist
            if (user == null || user.Actor == null)
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

    public async Task<ServiceResponse<int>> UpdateActorProfile(UpdateActorProfileRequestDto actorDto)
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
                .FirstOrDefaultAsync(u => u.Id == actorDto.UserId);

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
            if (existingUserWithActor.Name != actorDto.Name || existingUserWithActor.Email != actorDto.Email)
            {
                existingUserWithActor.Name = actorDto.Name;
                existingUserWithActor.Email = actorDto.Email;
                existingUserWithActor.LastUpdated = DateTime.UtcNow;
                _context.Users.Update(existingUserWithActor);
            }

            // Update Actor properties
            var existingActor = existingUserWithActor.Actor;
            if (existingActor.BirthDate != actorDto.BirthDate ||
                existingActor.Gender != actorDto.Gender ||
                existingActor.Height != actorDto.Height ||
                existingActor.Weight != actorDto.Weight ||
                existingActor.Bio != actorDto.Bio)
            {
                existingActor.BirthDate = actorDto.BirthDate;
                existingActor.Gender = actorDto.Gender;
                existingActor.Height = actorDto.Height;
                existingActor.Weight = actorDto.Weight;
                existingActor.Bio = actorDto.Bio;
                existingActor.LastUpdated = DateTime.UtcNow;
                _context.Actors.Update(existingActor);
            }

            // Update Education
            _context.ActorEducations.RemoveRange(existingActor.Education);
            if (actorDto.Education != null)
            {
                existingActor.Education = actorDto.Education.Select(e => new ActorEducation
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
            if (actorDto.Links != null)
            {
                existingActor.Links = actorDto.Links.Select(l => new ActorLink
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

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {actorDto.UserId}");

            return response;
        }
    }

}
