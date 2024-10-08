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

    public async Task<ServiceResponse<GetMainPageResponseDto>> GetMainPage(int? userId)
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
            var demoStarResponse = await GetDemoStars(userId);
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

    public async Task<ServiceResponse<GetDemoStarResponseDto>> GetDemoStar(int? userId, int demoStarId)
    {
        var response = new ServiceResponse<GetDemoStarResponseDto>();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var demoStar = await _context.ActorDemoStars
                .FirstOrDefaultAsync(ds => ds.Id == demoStarId);

            if (demoStar == null)
            {
                await transaction.RollbackAsync();

                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_DATA.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DATA];
                return response;
            }

            demoStar.ViewCount++;
            await _context.SaveChangesAsync();

            var demoStarData = await _context.ActorDemoStars
                .Where(ds => ds.Id == demoStarId)
                .Select(ds => new GetDemoStarResponseDto
                {
                    actorId = ds.ActorId,
                    actorName = ds.Actor.User.Name,
                    actorImageUrl = ds.Actor.User.PublicUrl,
                    demoStarData = new DemoStarData
                    {
                        demoStarId = ds.Id,
                        Title = ds.Title,
                        Category = ds.Category,
                        Url = ds.Url,
                        ViewCount = ds.ViewCount,
                        LikesCount = ds.LikesFromCommons + ds.LikesFromActors + ds.LikesFromEnters,
                        IsLikedByUser = userId.HasValue && _context.UserLikes.Any(ul =>
                                                ul.UserId == userId.Value &&
                                                ul.ItemId == ds.Id &&
                                                ul.ItemType == LikeType.DemoStar),
                        CreatedDate = ds.CreatedDate,
                        LastUpdated = ds.LastUpdated
                    }
                })
                .FirstOrDefaultAsync() ?? new GetDemoStarResponseDto();

            demoStarData.RecommendedDemoStars = await GetRecommendedDemoStars(demoStarId);

            await transaction.CommitAsync();

            response.Data = demoStarData;
            response.Success = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("GetDemoStar", ex.Message, ex.StackTrace ?? string.Empty);
        }

        return response;
    }

    // TO DO: 이거 SP로 하는게 더 좋을 수도 있어. db 여러번 왔다갔다하는것보다 SP 한번 처리한느게 더 나을거같은데. 가능하면?
    private async Task<List<RecommendedDemoStarDto>> GetRecommendedDemoStars(int currentDemoStarId)
    {
        // 현재 DemoStar의 정보를 가져옵니다.
        var currentDemoStar = await _context.ActorDemoStars
            .Where(ds => ds.Id == currentDemoStarId)
            .Select(ds => new { ds.Title, ds.Category })
            .FirstOrDefaultAsync();

        if (currentDemoStar == null)
        {
            return new List<RecommendedDemoStarDto>();
        }

        // 추천 DemoStars를 가져옵니다.
        var recommendedDemoStars = await _context.ActorDemoStars
            .Where(ds => ds.Id != currentDemoStarId)
            .Select(ds => new
            {
                ds.Id,
                ds.Title,
                ds.Url,
                ds.Category,
                ds.CreatedDate,
                ActorId = ds.ActorId,
                ds.ViewCount,
                ActorImageUrl = ds.Actor.User.PublicUrl
            })
            .ToListAsync();

        long maxViewCount = recommendedDemoStars.Max(ds => ds.ViewCount);

        // 메모리에서 관련성 점수를 계산하고 정렬합니다.
        var sortedRecommendations = recommendedDemoStars
            .Select(ds => new
            {
                DemoStar = ds,
                RelevanceScore = (ds.Category == currentDemoStar.Category ? 1.0 : 0) +
                                    NormalizedSimilarity(ds.Title.ToLower(), currentDemoStar.Title.ToLower()) +
                                    (maxViewCount > 0 ? (double)ds.ViewCount / maxViewCount * 2 : 0)
            })
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.DemoStar.CreatedDate)
            .Take(10)
            .Select(x => new RecommendedDemoStarDto
            {
                demoStarId = x.DemoStar.Id,
                title = x.DemoStar.Title,
                url = x.DemoStar.Url,
                actorId = x.DemoStar.ActorId,
                actorImageUrl = x.DemoStar.ActorImageUrl
            })
            .ToList();

        return sortedRecommendations;
    }

    public async Task<ServiceResponse<DemoStarDataResponseDto>> GetDemoStars(int? userId, string filter = "", string sortBy = "", int page = 1, int limit = 20)
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

            // 2. Count
            var totalCount = await query.CountAsync();

            // 3. sortby
            query = sortBy.ToLower() switch
            {
                "oldest" => query.OrderBy(ds => ds.LastUpdated),
                "latest" => query.OrderByDescending(ds => ds.LastUpdated),
                _ => query.OrderByDescending(ds => ds.Id)
            };

            // 4. pagination
            var totalPages = (int)Math.Ceiling((double)totalCount / limit);
            var skip = (page - 1) * limit;

            var demoStarItems = await query
                .Skip(skip)
                .Take(limit)
                .Select(ds => new DemoStarItemDto
                {
                    Id = ds.Id,
                    Title = ds.Title,
                    Category = ds.Category,
                    Url = ds.Url,
                    ViewCount = ds.ViewCount,
                    LikesCount = ds.LikesFromCommons + ds.LikesFromActors + ds.LikesFromEnters,
                    IsLikedByUser = false
                })
                .ToListAsync();

            // 5. 현재 demostar list중에 해당 user가 like가 있는 리스트
            if (userId.HasValue)
            {
                var likedDemoStarIds = await _context.UserLikes
                    .Where(ul => ul.UserId == userId.Value &&
                                 ul.ItemType == LikeType.DemoStar &&
                                 demoStarItems.Select(ds => ds.Id).Contains(ul.ItemId))
                    .Select(ul => ul.ItemId)
                    .ToListAsync();

                var likedDemoStarIdSet = new HashSet<int>(likedDemoStarIds);

                foreach (var demoStar in demoStarItems)
                {
                    demoStar.IsLikedByUser = likedDemoStarIdSet.Contains(demoStar.Id);
                }
            }

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

    public async Task<ServiceResponse<GetActorPortfolioResponseDto>> GetAllActorPortfolios(int? userId, string filter = "", string sortBy = "", int page = 1, int limit = 50)
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
                        .FirstOrDefault() ?? string.Empty,
                    BookMarksCount = a.LikesFromCommons + a.LikesFromActors + a.LikesFromEnters,
                    IsBookMarkeeByUser = false
                })
                .ToListAsync();
        });

        // 5. 현재 demostar list중에 해당 user가 bookmark가 있는 리스트
        if (userId.HasValue)
        {
            var likedBookMarkIds = await _context.UserLikes
                .Where(ul => ul.UserId == userId.Value &&
                             ul.ItemType == LikeType.Portfolio &&
                             allActors.Select(ds => ds.ActorId).Contains(ul.ItemId))
                .Select(ul => ul.ItemId)
                .ToListAsync();

            var likedBookMarkIdSet = new HashSet<int>(likedBookMarkIds);

            foreach (var actor in allActors)
            {
                actor.IsBookMarkeeByUser = likedBookMarkIdSet.Contains(actor.ActorId);
            }
        }

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

    // TO DO: Levenshtein Distance : 데이터가 별로 없을 때 detail을 잡기위해 사용 -> 데이터가 많아지면 부하가 많아서 다른 방식으로 변경해야 됨!
    public static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    public static double NormalizedSimilarity(string s1, string s2)
    {
        int maxLength = Math.Max(s1.Length, s2.Length);
        if (maxLength == 0) return 1.0; // 두 문자열이 모두 빈 문자열인 경우
        return 1.0 - (double)LevenshteinDistance(s1, s2) / maxLength;
    }
}
