using NewFace.DTOs.Home;
using NewFace.Responses;

namespace NewFace.Services;

public interface ICommonActorService
{
    Task<ServiceResponse<GetDemoStarResponseDto>> GetDemoStar(int? userId, int demoStarId);
    Task<ServiceResponse<GetActorPortfolioDetailResponseDto>> GetActorPortfolio(int? userId, int actorId);
}
