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
    [Route("api/common/actor")]
    [ApiController]
    public class CommonActorController : ControllerBase
    {
        private readonly ICommonActorService _commonActorService;

        public CommonActorController(ICommonActorService commonActorService)
        {
            _commonActorService = commonActorService;
        }

        [SwaggerOperation(Summary = "데모스타 상세 페이지")]
        [HttpGet("demo-star")]
        public async Task<IActionResult> GetDemoStar([FromQuery] int demoStarId)
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

            var response = await _commonActorService.GetDemoStar(userId, demoStarId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [SwaggerOperation(Summary = "배우 포트폴리오 상세 페이지")]
        [HttpGet("portfolio")]
        public async Task<IActionResult> GetActorPortfolio([FromQuery] int actorId)
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

            var response = await _commonActorService.GetActorPortfolio(userId, actorId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
