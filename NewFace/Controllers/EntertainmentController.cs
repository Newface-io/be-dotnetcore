using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.DTOs.Actor;
using NewFace.Filters;
using NewFace.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

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

        // POST: api/enter/mypage/profile
        [HttpPut("mypage/profile")]
        [SwaggerOperation(Summary = "마이페이지 - 연예관계자 소속 정보 수정")]
        public async Task<IActionResult> UpdateEntertainmentProfile([FromBody] UpdateEntertainmentProfileRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _entertainmentService.UpdateEntertainmentProfile(model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
    }
}
