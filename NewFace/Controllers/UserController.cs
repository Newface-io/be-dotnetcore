using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.Common.Constants;
using NewFace.Filters;
using NewFace.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace NewFace.Controllers
{
    // AuthenticateAndValidate : model validation / user id 체크
    [Authorize]
    [Route("api/[controller]")]
    [AuthenticateAndValidate]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [SwaggerOperation(Summary = "유저 삭제")]
        // DELETE: api/user
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (currentUserId == userId)
            {
                var response = await _userService.DeleteUser(userId);

                if (!response.Success)
                {
                    if (response.Code == MessageCode.Custom.NOT_FOUND_USER.ToString())
                        return NotFound(response);

                    return StatusCode(500, response);
                }

                return Ok(response);
            } else
            {
                return StatusCode(500);
            }
        }


        [SwaggerOperation(Summary = "유저 Role 설정(일반 / 배우 / 엔터)")]
        [HttpPost]
        [Route("setUserRole")]
        public async Task<IActionResult> SetUserRole(int userId, string role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.SetUserRole(userId, role);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }
    }
}
