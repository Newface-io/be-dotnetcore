using NewFace.DTOs.Actor;
using NewFace.Models.Actor;
using NewFace.Responses;
namespace NewFace.Services;

public interface IActorService
{
    #region profile
    Task<ServiceResponse<GetActorResponseDto>> GetActorProfile(int userId, int actorId);
    Task<ServiceResponse<int>> UpdateActorProfile(int userId, int actorId, UpdateActorProfileRequestDto model);
    #endregion

    #region demo star
    Task<ServiceResponse<ActorDemoStarListDto>> GetActorDemoStar(int userId, int actorId);
    Task<ServiceResponse<int>> AddActorDemoStar(int userId, int actorId, AddActorDemoStarDto model);
    Task<ServiceResponse<int>> UpdateActorDemoStar(int userId, int actorId, UpdateActorDemoStarDto model);
    Task<ServiceResponse<int>> DeleteActorDemoStar(int userId, int actorId, int demoStarId);
    #endregion

    #region image
    Task<ServiceResponse<GetActorImagesResponseDto>> GetActorImages(int userId, int actorId);
    Task<ServiceResponse<GetActorImagesByGroupResponseDto>> GetActorImagesByGroup(int userId, int actorId, int groupId);
    Task<ServiceResponse<bool>> UploadActorImages(int userId, int actorId, UploadActorImagesRequestDto model);
    Task<ServiceResponse<bool>> DeleteActorImages(int userId, int actorId, List<int> groupIds);
    Task<ServiceResponse<int>> SetActorMainImage(int userId, int actorId, int groupId);
    #endregion

    #region experience

    Task<ServiceResponse<GetActorExperiencesResponseDto>> GetActorExperiences(int userId, int actorId);
    Task<ServiceResponse<bool>> UpdateActorExperiences(int userId, int actorId, UpdateActorExperiencesRequestDto model);
    #endregion
}
