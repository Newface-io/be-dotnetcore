﻿using Microsoft.AspNetCore.Mvc;
using NewFace.Common.Constants;
using NewFace.DTOs.Auth;
using NewFace.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace NewFace.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [SwaggerOperation(Summary = "로그인 - 이메일")]
        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> SignInEmail([FromBody] SignInRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.SignInEmail(request);

            if (!response.Success)
            {
                if (response.Code == MessageCode.Custom.NOT_FOUND_USER.ToString())
                {
                    return NotFound(response);
                }
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [SwaggerOperation(Summary = "로그인 - 카카오")]
        [HttpGet("signin/kakao")]
        public IActionResult SignInKakao()
        {
            var loginUrl = _authService.GetKakaoLoginUrl();
            return Redirect(loginUrl);
        }


        [SwaggerOperation(Summary = "로그인(토큰처리) - 카카오")]
        [HttpPost("signin/kakao/callback")]
        public async Task<IActionResult> SignInKakaoCallback([FromQuery] string code)
        {
            var token = await _authService.GetKakaoToken(code);
            var kakaoUserInfo = _authService.GetKakaoUserInfo(token);

            // TO DO: 계정 만들었는지 체크하고 없으면 signup하고 아니면 로그인 처리
            return Ok(token);
        }

        [SwaggerOperation(Summary = "네이버 로그인")]
        [HttpGet("naver/signin")]
        public IActionResult NaverLogin()
        {
            var loginUrl = _authService.GetNaverLoginUrl();
            return Ok(new { loginUrl });
        }

        [SwaggerOperation(Summary = "네이버 로그인(인증 후)")]
        [HttpGet("naver/signin/callback")]
        public async Task<IActionResult> NaverLoginCallback([FromQuery] string code)
        {
            var getTokenResponse = await _authService.GetNaverToken(code);

            if (getTokenResponse.Success && getTokenResponse.Data != null)
            {
                var getUserInfo = await _authService.GetNaverUserInfo(getTokenResponse.Data);

                if (getUserInfo.Success && getUserInfo.Data != null && getUserInfo.Data.id != null)
                {
                    var isCompletedResponse = await _authService.IsCompleted(getUserInfo.Data.id, USER_AUTH.NAVER);

                    if (isCompletedResponse.Success && isCompletedResponse.Data != null)
                    {
                        // 1. get user info and token
                        if (isCompletedResponse.Data.isCompleted)
                        {
                            var response = await _authService.SignInNaver(getUserInfo.Data.id);

                            if (!response.Success)
                            {
                                return StatusCode(500, response);
                            }

                            return Ok(response);
                        }
                        // 2. response user info from Naver
                        else
                        {
                            return Ok(isCompletedResponse);
                        }
                    }

                    return StatusCode(500, isCompletedResponse);
                }
                else
                {
                    return StatusCode(500, getUserInfo);
                }

            }
            else
            {
                return StatusCode(500, getTokenResponse);
            }
        }


        [SwaggerOperation(Summary = "회원가입 - 이메일")]
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUpEmail([FromBody] SignUpRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.SignUpEmail(request);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }


        [SwaggerOperation(Summary = "핸드폰 OTP 번호 발송")]
        [HttpPost]
        [Route("send-otp")]
        public async Task<IActionResult> SendOTP(string phone)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.SendOTP(phone);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }


        [SwaggerOperation(Summary = "핸드폰 OTP 번호 확인")]
        [HttpPost]
        [Route("verify-otp")]
        public async Task<IActionResult> VerifyOTP(string phone, string inputOTP)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.VerifyOTP(phone, inputOTP);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }

}
