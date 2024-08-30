using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;
namespace NewFace.Services;

public interface IActorService
{
    Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId);
    Task<ServiceResponse<int>> AddActorProfile(AddActorProfileRequestDto actor);
    Task<ServiceResponse<int>> UpdateActorProfile(int actorId, AddActorProfileRequestDto actor);
}
