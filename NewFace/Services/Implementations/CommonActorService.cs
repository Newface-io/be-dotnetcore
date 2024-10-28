using Microsoft.EntityFrameworkCore;
using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Actor;
using NewFace.DTOs.Home;
using NewFace.Responses;

namespace NewFace.Services;

public class CommonActorService : ICommonActorService
{
    private readonly DataContext _context;
    private readonly ILogService _logService;
    private readonly IMemoryManagementService _memoryManagementService;

    public CommonActorService(DataContext context, ILogService logService, IMemoryManagementService memoryManagementService)
    {
        _context = context;
        _logService = logService;
        _memoryManagementService = memoryManagementService;
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

    public async Task<ServiceResponse<GetActorPortfolioDetailResponseDto>> GetActorPortfolio(int? userId, int actorId)
    {
        var response = new ServiceResponse<GetActorPortfolioDetailResponseDto>();

        try
        {
            var isBookmarked = false;
            var likedDemoStars = new HashSet<int>();

            if (userId.HasValue)
            {
                var userLikes = await _context.UserLikes
                    .Where(ul => ul.UserId == userId.Value &&
                                 (ul.ItemType == LikeType.Portfolio || ul.ItemType == LikeType.DemoStar))
                    .ToListAsync();

                isBookmarked = userLikes.Any(ul => ul.ItemType == LikeType.Portfolio && ul.ItemId == actorId);

                likedDemoStars = new HashSet<int>(userLikes
                    .Where(ul => ul.ItemType == LikeType.DemoStar)
                    .Select(ul => ul.ItemId));
            }


            var actorInfo = await _context.Users
                                    .Include(u => u.Actor)
                                    .Where(u => u.Actor != null && u.Actor.Id == actorId)
                                    .Select(u => new
                                    {
                                        User = u,
                                        Actor = u.Actor
                                    })
                                    .FirstOrDefaultAsync();

            if (actorInfo == null)
            {
                response.Success = false;
                response.Code = MessageCode.Custom.INVALID_DEMOSTAR.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.INVALID_DEMOSTAR];
                return response;
            }

            var links = await _context.ActorLinks
            .Where(l => l.ActorId == actorId)
            .Select(l => new LinkPortfolioDetailResponseDto
            {
                LinkId = l.Id,
                Category = l.Category,
                Url = l.Url
            })
            .ToListAsync();

            var education = await _context.ActorEducations
                .Where(e => e.ActorId == actorId)
                .Select(e => new EducationPortfolioDetailResponseDto
                {
                    EducationId = e.Id,
                    EducationType = e.EducationType,
                    GraduationStatus = e.GraduationStatus,
                    School = e.School,
                    Major = e.Major
                })
                .ToListAsync();

            var experiences = await _context.ActorExperiences
                .Where(e => e.ActorId == actorId)
                .Select(e => new ExperiencePortfolioDetailResponseDto
                {
                    ExperienceId = e.Id,
                    Category = e.Category,
                    WorkTitle = e.WorkTitle
                })
                .ToListAsync();

            var demoStars = await _context.ActorDemoStars
                .Where(ds => ds.ActorId == actorId)
                .Select(ds => new DemoStarPortfolioDetailResponseDto
                {
                    DemoStarId = ds.Id,
                    Title = ds.Title,
                    Category = ds.Category,
                    Url = ds.Url,
                    IsLikedByUser = userId.HasValue && likedDemoStars.Contains(ds.Id)
                })
                .ToListAsync();

            // 2. get image
            var actorImages = await _context.ActorImages
                                .Where(ai => ai.ActorId == actorId && ai.IsDeleted == false)
                                .GroupBy(ai => ai.GroupId)
                                .Select(g => new
                                {
                                    GroupId = g.Key,
                                    FirstImage = g.OrderBy(ai => ai.GroupOrder).First(),    // group의 첫번째 이미지
                                    ImageCount = g.Count(),                                 // group에서 이미지 개수
                                    ActorOrder = g.First().ActorOrder,                      // group의 첫번째 이미지 Order (어차피 groupId가 같으면 ActorOrder는 동일)
                                    IsMainImage = g.Any(ai => ai.IsMainImage)
                                })
                                .OrderBy(g => g.ActorOrder)
                                .ToListAsync();

            var images = actorImages.Select(g => new GetActorImages
            {
                ImageId = g.FirstImage.Id,
                ImageUrl = g.FirstImage.PublicUrl,
                FileName = g.FirstImage.FileName,
                GroupId = g.GroupId,
                GroupOrder = g.FirstImage.GroupOrder,
                ImageCount = g.ImageCount,
                IsMainImage = g.IsMainImage
            }).ToList();

            response.Data = new GetActorPortfolioDetailResponseDto
            {
                UserId = actorInfo.User.Id,
                Name = actorInfo.User.Name,
                Email = actorInfo.User.Email,
                Gender = actorInfo.User.Gender ?? string.Empty,
                BirthDate = actorInfo.User.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                Height = actorInfo.Actor.Height?.ToString() ?? string.Empty,
                Weight = actorInfo.Actor.Weight?.ToString() ?? string.Empty,
                Bio = actorInfo.Actor.Bio ?? string.Empty,
                ActorId = actorInfo.Actor.Id,
                IsBookMarkeeByUser = isBookmarked,
                ActorLinks = links,
                ActorEducations = education,
                ActorExperiences = experiences,
                ActorImages = images,
                ActorDemoStars = demoStars
            };


            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("GetActorPortfolio", ex.Message, ex.StackTrace ?? string.Empty);
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
