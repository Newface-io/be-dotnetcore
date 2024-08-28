using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.Models.Actor;
using NewFace.Services;

namespace NewFace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly IActorService _actorService;
        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        // POST: api/actor/profile
        [HttpPost("profile")]
        public async Task<IActionResult> AddPostActorProfile([FromBody] Actor model)
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

        // PUT: api/actor/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateActorProfile([FromBody] Actor model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _actorService.UpdateActorProfile(model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }
}
