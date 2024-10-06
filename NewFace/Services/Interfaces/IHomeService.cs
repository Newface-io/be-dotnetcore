using NewFace.DTOs.Home;
using NewFace.Responses;

namespace NewFace.Services.Interfaces;

public interface IHomeService
{
    Task<ServiceResponse<GetMainPageResponseDto>> GetMainPage();
    Task<ServiceResponse<DemoStarDataResponseDto>> GetDemoStars(string filter = "", string sortBy = "", int page = 1, int limit = 20);
    Task<ServiceResponse<GetActorPortfolioResponseDto>> GetAllActorPortfolios(string filter = "", string sortBy = "", int page = 1, int limit = 50);
    Task<ServiceResponse<GetDemoStarResponseDto>> GetDemoStar(int demoStarId);
}
