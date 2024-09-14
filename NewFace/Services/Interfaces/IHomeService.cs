using NewFace.DTOs.Home;
using NewFace.Responses;

namespace NewFace.Services.Interfaces;

public interface IHomeService
{
    Task<ServiceResponse<GetMainPageResponseDto>> GetMainPage();
    Task<ServiceResponse<DemoStarDataResponseDto>> GetDemoStars(string category = "", string sortBy = "", int page = 1, int limit = 20);
}
