using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.DTOs.Actor;
using NewFace.Services;
using System.Security.Claims;
using NewFace.Filters;
using Swashbuckle.AspNetCore.Annotations;

namespace NewFace.Controllers
{

    // AuthenticateAndValidate : model validation / user id 체크
    [Authorize]
    [Route("api/[controller]")]
    [AuthenticateAndValidate]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly IActorService _actorService;
        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        // GET: api/actor/profile/123
        [SwaggerOperation(Summary = "배우 프로필 조회")]
        [HttpGet("profile/{actorId}")]
        public async Task<IActionResult> GetActorProfile([FromRoute] int actorId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.GetActorProfile(userId, actorId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        // PUT: api/actor/profile/123?UpdateActorProfileRequestDto
        [SwaggerOperation(Summary = "배우 프로필 수정")]
        [HttpPut("profile/{actorId}")]
        public async Task<IActionResult> UpdateActorProfile([FromRoute] int actorId, [FromBody] UpdateActorProfileRequestDto model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.UpdateActorProfile(userId, actorId, model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [SwaggerOperation(Summary = "배우 데모스타 목록")]
        [HttpGet("demostar/{actorId}")]
        public async Task<IActionResult> GetActorDemoStarList([FromRoute] int actorId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.GetActorDemoStar(userId, actorId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [SwaggerOperation(Summary = "배우 데모스타 추가")]
        [HttpPost("demostar/{actorId}")]
        public async Task<IActionResult> AddActorDemoStar([FromRoute] int actorId, [FromBody] AddActorDemoStarDto model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.AddActorDemoStar(userId, actorId, model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [SwaggerOperation(Summary = "배우 데모스타 수정")]
        [HttpPut("demostar/{actorId}")]
        public async Task<IActionResult> UpdateActorDemoStar([FromRoute] int actorId, [FromBody] UpdateActorDemoStarDto model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.UpdateActorDemoStar(userId, actorId, model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [SwaggerOperation(Summary = "배우 데모스타 삭제")]
        [HttpDelete("demostar/{actorId}")]
        public async Task<IActionResult> DeleteActorDemoStar([FromRoute] int actorId, int demoStarId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.DeleteActorDemoStar(userId, actorId, demoStarId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }
}
