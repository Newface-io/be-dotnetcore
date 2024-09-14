using Microsoft.EntityFrameworkCore;
using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Home;
using NewFace.Models.Actor;
using NewFace.Responses;
using NewFace.Services.Interfaces;

namespace NewFace.Services;

public class HomeService : IHomeService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;

    public HomeService(DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<ServiceResponse<GetMainPageResponseDto>> GetMainPage()
    {
        var response = new ServiceResponse<GetMainPageResponseDto>();

        try
        {
            var mainPageData = new GetMainPageResponseDto();

            // 1. Get top 10 actors
            mainPageData.TopActorData = await _context.Actors
                .Where(a => a.Images.Any(i => i.IsMainImage))
                .OrderByDescending(a => a.LastUpdated)
                .Take(10)
                .Select(a => new ActorTopDto
                {
                    ActorId = a.Id,
                    Name = a.User.Name,
                    ImageUrl = a.User.ImageUrl,
                    Bio = a.Bio,
                    MainActorImageUrl = a.Images.FirstOrDefault(i => i.IsMainImage).StoragePath
                })
                .ToListAsync();

            // 2. Get DemoStar data
            var demoStarResponse = await GetDemoStars();
            if (demoStarResponse.Success)
            {
                mainPageData.DemoStarData = demoStarResponse.Data;
            }
            else
            {
                mainPageData.DemoStarData = new DemoStarDataResponseDto { TotalCount = 0, DamoStars = new List<DemoStarItemDto>() };
            }

            response.Data = mainPageData;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "An error occurred while fetching main page data.";
            _logService.LogError("GetMainPageData", ex.Message, ex.StackTrace);
        }

        return response;
    }

    public async Task<ServiceResponse<DemoStarDataResponseDto>> GetDemoStars(string category = "", string sortBy = "", int page = 1, int limit = 20)
    {
        var response = new ServiceResponse<DemoStarDataResponseDto>();

        try
        {
            IQueryable<ActorDemoStar> query = _context.ActorDemoStars;

            // 1. filter : category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(ds => ds.Category == category);
            }

            // 2. sortby
            switch (sortBy.ToLower())
            {
                case "oldest":
                    query = query.OrderBy(ds => ds.LastUpdated);
                    break;
                case "latest":
                    query = query.OrderByDescending(ds => ds.LastUpdated);
                    break;
                default:
                    query = query.OrderByDescending(ds => ds.Id);
                    break;
            }

            // 3. pagenation
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / limit);
            var skip = (page - 1) * limit;

            var demoStarItems = await query
                .Skip(skip)
                .Take(limit)
                .Select(ds => new DemoStarItemDto
                {
                    Title = ds.Title,
                    Category = ds.Category,
                    Url = ds.Url
                })
                .ToListAsync();

            response.Data = new DemoStarDataResponseDto
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = limit,
                DamoStars = demoStarItems
            };

            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("GetDemoStarData", ex.Message, ex.StackTrace);
        }

        return response;
    }
}
