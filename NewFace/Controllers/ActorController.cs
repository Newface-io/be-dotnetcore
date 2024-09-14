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

    // AuthenticateAndValidateActor : model validation / user id, actor id 체크
    [Authorize(Roles = NewFace.Common.Constants.UserRole.Actor)]
    [Route("api/actor")]
    [AuthenticateAndValidateActor]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly IActorService _actorService;
        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        // GET: api/actor/profile/123
        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 프로필 조회", Tags = new[] { "Actor/Profile" })]
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
        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 프로필 수정", Tags = new[] { "Actor/Profile" })]
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

        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 데모스타 목록", Tags = new[] { "Actor/DemoStar" })]
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


        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 데모스타 추가", Tags = new[] { "Actor/DemoStar" })]
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


        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 데모스타 수정", Tags = new[] { "Actor/DemoStar" })]
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


        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 데모스타 삭제", Tags = new[] { "Actor/DemoStar" })]
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

        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 사진 목록", Tags = new[] { "Actor/Image" })]
        [HttpGet("image/{actorId}")]
        public async Task<IActionResult> GetActorImages([FromRoute] int actorId)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.GetActorImages(userId, actorId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }

        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 사진 그룹 목록", Tags = new[] { "Actor/Image" })]
        [HttpGet("image/{actorId}/group/{groupId}")]
        public async Task<IActionResult> GetActorImagesByGroup([FromRoute] int actorId, [FromRoute]  int groupId)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.GetActorImagesByGroup(userId, actorId, groupId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }


        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 사진 업로드", Tags = new[] { "Actor/Image" })]
        [HttpPost("image/{actorId}")]
        public async Task<IActionResult> UploadActorImages([FromRoute] int actorId, [FromForm] UploadActorImagesRequestDto model)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.UploadActorImages(userId, actorId, model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }

        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 사진 그룹 삭제", Tags = new[] { "Actor/Image" })]
        [HttpDelete("image/{actorId}")]
        public async Task<IActionResult> DeleteActorImages([FromRoute] int actorId, [FromBody] List<int> groupIds)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.DeleteActorImages(userId, actorId, groupIds);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }

        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 대표 사진 설정", Tags = new[] { "Actor/Image" })]
        [HttpPut("image/{actorId}/set-representative/{groupId}")]
        public async Task<IActionResult> SetActorMainImage([FromRoute] int actorId, [FromRoute] int groupId)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.SetActorMainImage(userId, actorId, groupId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }


        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 작품활동 목록", Tags = new[] { "Actor/Experience" })]
        [HttpGet("experience/{actorId}")]
        public async Task<IActionResult> GetActorExperiences([FromRoute] int actorId)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.GetActorExperiences(userId, actorId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }


        [SwaggerOperation(Summary = "[포트폴리오 추가/수정] 배우 작품활동 수정", Tags = new[] { "Actor/Experience" })]
        [HttpPut("experience/{actorId}")]
        public async Task<IActionResult> UpdateActorExperiences([FromRoute] int actorId, [FromBody]UpdateActorExperiencesRequestDto model)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var response = await _actorService.UpdateActorExperiences(userId, actorId, model);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }
    }
}
