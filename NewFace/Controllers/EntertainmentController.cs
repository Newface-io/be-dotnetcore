using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.DTOs.Actor;
using NewFace.Filters;
using NewFace.Services.Interfaces;

namespace NewFace.Controllers
{
    // AuthenticateAndValidateEntertainment : model validation / user id, enter id 체크
    [Authorize(Roles = NewFace.Common.Constants.USER_ROLE.ENTER)]
    [AuthenticateAndValidateEntertainment]
    [Route("api/enter")]
    [ApiController]
    public class EntertainmentController : ControllerBase
    {
        private readonly IEntertainmentService _entertainmentService;
        public EntertainmentController(IEntertainmentService entertainmentService)
        {
            _entertainmentService = entertainmentService;
        }

        // POST: api/entertainment/profile
        [HttpPost("profile")]
        public async Task<IActionResult> AddEntertainmentProfile([FromBody] AddEntertainmentProfileRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _entertainmentService.AddEntertainmentProfile(model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
    }
}
