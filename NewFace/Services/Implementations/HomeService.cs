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
    private readonly IMemoryManagementService _memoryManagementService;

    public HomeService(DataContext context, ILogService logService, IMemoryManagementService memoryManagementService)
    {
        _context = context;
        _logService = logService;
        _memoryManagementService = memoryManagementService;
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
                    ImageUrl = a.User.PublicUrl,
                    Bio = a.Bio,
                    MainActorImageUrl = a.Images
                                        .Where(i => i.IsMainImage)
                                        .Select(i => i.PublicUrl)
                                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            // 2. Get DemoStar data
            var demoStarResponse = await GetDemoStars();
            if (demoStarResponse.Success)
            {
                mainPageData.DemoStarData = demoStarResponse.Data?? new DemoStarDataResponseDto { TotalCount = 0, DamoStars = new List<DemoStarItemDto>() };
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
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("GetMainPageData", ex.Message, ex.StackTrace ?? string.Empty) ;
        }

        return response;
    }

    public async Task<ServiceResponse<DemoStarDataResponseDto>> GetDemoStars(string filter = "", string sortBy = "", int page = 1, int limit = 20)
    {
        var response = new ServiceResponse<DemoStarDataResponseDto>();

        try
        {

            IQueryable<ActorDemoStar> query = _context.ActorDemoStars.AsNoTracking();

            // 1. Filter
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(ds => ds.Category == filter);
            }

            // 2. sortby
            query = sortBy.ToLower() switch
            {
                "oldest" => query.OrderBy(ds => ds.LastUpdated),
                "latest" => query.OrderByDescending(ds => ds.LastUpdated),
                _ => query.OrderByDescending(ds => ds.Id)
            };

            // 3. pagenation
            var totalCount = await _context.ActorDemoStars
                                        .Where(ds => string.IsNullOrEmpty(filter) || ds.Category == filter)
                                        .CountAsync();

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

            _logService.LogError("GetDemoStarData", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }

    public async Task<ServiceResponse<GetActorPortfolioResponseDto>> GetAllActorPortfolios(string filter = "", string sortBy = "", int page = 1, int limit = 50)
    {
        var allActors = await _memoryManagementService.GetOrSetCache("AllActorPortfolios", async () =>
        {
            return await _context.Actors
                .Where(a => a.Images.Any(i => i.IsMainImage))
                .Select(a => new ActorPortfolioDto
                {
                    ActorId = a.Id,
                    Name = a.User.Name,
                    BirthDate = a.User.BirthDate,
                    Gender = a.User.Gender,
                    Age = a.User.BirthDate.HasValue ? DateTime.Now.Year - a.User.BirthDate.Value.Year : null,
                    CreatedDate = a.CreatedDate,
                    LastUpdated = a.Images
                        .Where(i => i.IsMainImage)
                        .OrderBy(i => i.GroupOrder)
                        .Select(i => i.LastUpdated)
                        .FirstOrDefault(),
                    MainImageUrl = a.Images
                        .Where(i => i.IsMainImage)
                        .Select(i => i.StoragePath)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();
        });

        var response = new ServiceResponse<GetActorPortfolioResponseDto>();

        try
        {
            // 필터링 적용
            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter.Trim().ToLower())
                {
                    case "male":
                        allActors = allActors.Where(a => a.Gender == "male").ToList();
                        break;
                    case "female":
                        allActors = allActors.Where(a => a.Gender == "female").ToList();
                        break;
                    default:
                        break;
                }
            }

            // 정렬 적용
            switch (sortBy.ToLower())
            {
                case "age_asc":
                    allActors = allActors.OrderBy(a => a.Age).ToList();
                    break;
                case "age_desc":
                    allActors = allActors.OrderByDescending(a => a.Age).ToList();
                    break;
                case "updated_asc":
                    allActors = allActors.OrderBy(a => a.LastUpdated).ToList();
                    break;
                case "updated_desc":
                    allActors = allActors.OrderByDescending(a => a.LastUpdated).ToList();
                    break;
                default:
                    allActors = allActors.OrderByDescending(a => a.CreatedDate).ToList();
                    break;
            }

            var totalCount = allActors.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / limit);
            var pagedActors = allActors.Skip((page - 1) * limit).Take(limit).ToList();

            response.Data = new GetActorPortfolioResponseDto
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = limit,
                Actors = pagedActors
            };

            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }
}
