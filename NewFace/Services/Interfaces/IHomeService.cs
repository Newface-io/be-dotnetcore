using NewFace.DTOs.Home;
using NewFace.Responses;

namespace NewFace.Services.Interfaces;

public interface IHomeService
{
    Task<ServiceResponse<GetMainPageResponseDto>> GetMainPage(int? userId);
    Task<ServiceResponse<DemoStarDataResponseDto>> GetDemoStars(int? userId, string filter = "", string sortBy = "", int page = 1, int limit = 20);
    Task<ServiceResponse<GetActorPortfolioResponseDto>> GetAllActorPortfolios(int? userId, string filter = "", string sortBy = "", int page = 1, int limit = 50);
}
