using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.Services;
using NewFace.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace NewFace.Controllers
{
    [AllowAnonymous]
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [SwaggerOperation(Summary = "메인 페이지 (전체)")]
        [HttpGet("main-page")]
        public async Task<IActionResult> Index()
        {
            int? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var response = await _homeService.GetMainPage(userId);
            if (!response.Success)
            {
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [SwaggerOperation(Summary = "메인 페이지 - 데모스타 목록 (페이지네이션)")]
        [HttpGet("demo-stars")]
        public async Task<IActionResult> GetDemoStars([FromQuery] string filter = "", [FromQuery] string sortBy = "", [FromQuery] int page = 1)
        {
            int? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var response = await _homeService.GetDemoStars(userId, filter, sortBy, page);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [SwaggerOperation(Summary = "메인 페이지 - 배우 포트폴리오 탭")]
        [HttpGet("portfolios")]
        public async Task<IActionResult> GetAllActorPortfolios([FromQuery] string filter = "", [FromQuery] string sortBy = "", [FromQuery] int page = 1)
        {
            var response = await _homeService.GetAllActorPortfolios(filter, sortBy, page);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [SwaggerOperation(Summary = "메인 페이지 - 데모스타 Detail")]
        [HttpGet("demo-star")]
        public async Task<IActionResult> GetDemoStar([FromQuery] int demoStarId)
        {
            var response = await _homeService.GetDemoStar(demoStarId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
