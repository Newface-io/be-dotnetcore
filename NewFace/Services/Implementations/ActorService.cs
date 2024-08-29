﻿using Microsoft.EntityFrameworkCore;
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
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
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

    public async Task<ServiceResponse<int>> AddActorProfile(Actor actor)
    {
        var response = new ServiceResponse<int>();

        try
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == actor.UserId);
            if (!userExists)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            await _context.Actors.AddAsync(actor);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Data = actor.UserId;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, "user id: " + actor.UserId + ", actor id:" + actor.Id);

            return response;
        }
    }

    public async Task<ServiceResponse<int>> UpdateActorProfile(Actor actor)
    {
        var response = new ServiceResponse<int>();

        try
        {
            var existingActor = await _context.Actors
                                                .Include(a => a.User)
                                                .FirstOrDefaultAsync(a => a.Id == actor.Id);

            if (existingActor == null || existingActor.User == null || existingActor.User.Id != actor.UserId)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];
                return response;
            }

            _context.Entry(existingActor).CurrentValues.SetValues(actor);

            await _context.SaveChangesAsync();

            response.Success = true;
            response.Data = actor.UserId;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, "user id: " + actor.UserId + ", actor id:" + actor.Id);

            return response;
        }
    }

}
