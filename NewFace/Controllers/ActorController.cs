using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.DTOs.Actor;
using NewFace.Services;

namespace NewFace.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly IActorService _actorService;
        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        // GET: api/actor/profile/1
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetActorProfile(int userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _actorService.GetActor(userId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        // POST: api/actor/profile
        [HttpPost("profile")]
        public async Task<IActionResult> AddActorProfile([FromBody] AddActorProfileRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _actorService.AddActorProfile(model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        // PUT: api/actors/profile/123
        [HttpPut("profile/{actorId}")]
        public async Task<IActionResult> UpdateActorProfile(int actorId, [FromBody] AddActorProfileRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _actorService.UpdateActorProfile(actorId, model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }
}
