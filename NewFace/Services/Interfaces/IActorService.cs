using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;
namespace NewFace.Services;

public interface IActorService
{
    Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId, int actorId);
    Task<ServiceResponse<int>> UpdateActorProfile(UpdateActorProfileRequestDto model);
    Task<ServiceResponse<ActorDemoStarListDto>> GetActorDemoStar(int userId, int actorId);
    Task<ServiceResponse<int>> AddActorDemoStar(AddActorDemoStarDto model);
    Task<ServiceResponse<int>> UpdateActorDemoStar(UpdateActorDemoStarDto model);
    Task<ServiceResponse<int>> DeleteActorDemoStar(int id, int userId);
}
