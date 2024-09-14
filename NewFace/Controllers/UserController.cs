﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.Common.Constants;
using NewFace.DTOs.User;
using NewFace.Filters;
using NewFace.Responses;
using NewFace.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace NewFace.Controllers
{
    // AuthenticateAndValidateUser : model validation / user id 체크
    [Authorize]
    [Route("api/[controller]")]
    [AuthenticateAndValidateUser]
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
                return Forbid();
            }
        }


        [SwaggerOperation(Summary = "유저 Role 설정(일반 / 배우 / 엔터)")]
        [HttpPost]
        [Route("setUserRole")]
        public async Task<IActionResult> SetUserRole(int userId, string role)
        {
            var response = await _userService.SetUserRole(userId, role);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [SwaggerOperation(Summary = "마이페이지 정보 조회")]
        [HttpGet("mypage")]
        public async Task<IActionResult> GetMyPageInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userId = int.Parse(userIdClaim.Value);
            var userRole = roleClaim.Value;

            int? roleSpecificId = null;

            if (userRole == NewFace.Common.Constants.UserRole.Actor)
            {
                roleSpecificId = User.FindFirst("ActorId")?.Value != null ? int.Parse(User.FindFirst("ActorId").Value) : null;
            }
            else if (userRole == NewFace.Common.Constants.UserRole.Entertainment)
            {
                roleSpecificId = User.FindFirst("EnterId")?.Value != null ? int.Parse(User.FindFirst("EnterId").Value) : null;
            }

            var response = await _userService.GetMyPageInfo(userId, userRole, roleSpecificId);
            return HandleResponse(response);
        }

        [HttpGet("mypage/edit")]
        [SwaggerOperation(Summary = "회원정보 수정 페이지 조회")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            var userId = int.Parse(userIdClaim.Value);

            var response = await _userService.GettUserInfoForEdit(userId);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        private IActionResult HandleResponse(ServiceResponse<IGetMyPageInfoResponseDto> response)
        {
            if (!response.Success)
            {
                if (response.Code == MessageCode.Custom.NOT_FOUND_USER.ToString())
                    return NotFound(response);

                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }
}
