using NewFace.DTOs.Actor;
using NewFace.Responses;

namespace NewFace.Services.Interfaces;

public interface IEntertainmentService
{
    Task<ServiceResponse<int>> UpdateEntertainmentProfile(UpdateEntertainmentProfileRequestDto model);
}
