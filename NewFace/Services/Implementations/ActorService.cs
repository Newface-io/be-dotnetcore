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

    public ActorService(DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<ServiceResponse<GetActorResponseDto>> GetActor(int userId)
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

            if (user == null || user.Actor == null)
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
                ActorEducations = user.Actor.Education.ToList(),
                ActorExperiences = user.Actor.Experiences.ToList(),
                ActorLinks = user.Actor.Links.ToList()
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

    public async Task<ServiceResponse<int>> AddActorProfile(AddActorProfileRequestDto actorDto)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var actor = new Actor
            {
                UserId = actorDto.UserId,
                Address = actorDto.Address,
                BirthDate = actorDto.BirthDate,
                Weight = actorDto.Weight,
                Height = actorDto.Height,
                Bio = actorDto.Bio,
            };

            await _context.Actors.AddAsync(actor);
            await _context.SaveChangesAsync();

            // Add Experiences
            if (actorDto.Experiences != null)
            {
                var experiences = actorDto.Experiences.Select(exp => new ActorExperience
                {
                    ActorId = actor.Id,
                    Category = exp.Category,
                    WorkTitle = exp.WorkTitle,
                    Role = exp.Role,
                    StartDate = exp.StartDate,
                    EndDate = exp.EndDate
                }).ToList();

                await _context.ActorExperiences.AddRangeAsync(experiences);
            }

            // Add Education
            if (actorDto.Education != null)
            {
                var education = actorDto.Education.Select(edu => new ActorEducation
                {
                    ActorId = actor.Id,
                    EducationType = edu.EducationType,
                    GraduationStatus = edu.GraduationStatus,
                    School = edu.School,
                    Major = edu.Major
                }).ToList();

                await _context.ActorEducations.AddRangeAsync(education);
            }

            // Add Links
            if (actorDto.Links != null)
            {
                var links = actorDto.Links.Select(link => new ActorLink
                {
                    ActorId = actor.Id,
                    Url = link.Url,
                    Description = link.Description
                }).ToList();

                await _context.ActorLinks.AddRangeAsync(links);
            }

            // Save all related entities
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            response.Success = true;
            response.Data = actor.Id;
            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, "user id: " + actorDto.UserId);

            return response;
        }
    }

    public async Task<ServiceResponse<int>> UpdateActorProfile(int actorId, AddActorProfileRequestDto actorDto)
    {
        var response = new ServiceResponse<int>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingActor = await _context.Users
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Experiences)
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Education)
                .Include(u => u.Actor)
                    .ThenInclude(a => a.Links)
                .Where(u => u.Actor.Id == actorId && u.Id == actorDto.UserId)
                .Select(u => u.Actor)
                .FirstOrDefaultAsync();

            if (existingActor == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            // Update actor properties
            existingActor.BirthDate = actorDto.BirthDate;
            existingActor.Address = actorDto.Address;
            existingActor.Height = actorDto.Height;
            existingActor.Weight = actorDto.Weight;
            existingActor.Bio = actorDto.Bio;

            // Update Experiences
            _context.ActorExperiences.RemoveRange(existingActor.Experiences);
            if (actorDto.Experiences != null)
            {
                existingActor.Experiences = actorDto.Experiences.Select(e => new ActorExperience
                {
                    ActorId = actorId,
                    Category = e.Category,
                    WorkTitle = e.WorkTitle,
                    Role = e.Role,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate
                }).ToList();
            }

            // Update Education
            _context.ActorEducations.RemoveRange(existingActor.Education);
            if (actorDto.Education != null)
            {
                existingActor.Education = actorDto.Education.Select(e => new ActorEducation
                {
                    ActorId = actorId,
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
                    ActorId = actorId,
                    Url = l.Url,
                    Description = l.Description
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

            _logService.LogError("EXCEPTION", ex.Message, $"user id: {actorDto.UserId}, actor id: {actorId}");

            return response;
        }
    }

}
