using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;
namespace NewFace.Services;

public interface IActorService
{
    Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId, int actorId);
    Task<ServiceResponse<int>> UpdateActorProfile(int userId, int actorId, UpdateActorProfileRequestDto model);
    Task<ServiceResponse<ActorDemoStarListDto>> GetActorDemoStar(int userId, int actorId);
    Task<ServiceResponse<int>> AddActorDemoStar(int userId, int actorId, AddActorDemoStarDto model);
    Task<ServiceResponse<int>> UpdateActorDemoStar(int userId, int actorId, UpdateActorDemoStarDto model);
    Task<ServiceResponse<int>> DeleteActorDemoStar(int userId, int actorId, int demoStarId);
}
