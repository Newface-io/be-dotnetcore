using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;
namespace NewFace.Services;

public interface IActorService
{
    Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId);
    Task<ServiceResponse<int>> UpdateActorProfile(UpdateActorProfileRequestDto actor);
}
