﻿using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;
namespace NewFace.Services;

public interface IActorService
{
    Task<ServiceResponse<GetActorResponseDto>> GetActor(int userId);
    Task<ServiceResponse<int>> AddActorProfile(Actor actor);
    Task<ServiceResponse<int>> UpdateActorProfile(Actor actor);
}
