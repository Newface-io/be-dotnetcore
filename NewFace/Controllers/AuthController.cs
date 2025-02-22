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


        [SwaggerOperation(Summary = "로그인 - 카카오 / 네이버")]
        [HttpGet("signin/{loginType}")]
        public IActionResult SignIn(string loginType)
        {
            string loginUrl;

            string formattedLoginType = loginType.Length > 1
                                        ? char.ToUpper(loginType[0]) + loginType.Substring(1).ToLower()
                                        : loginType.ToUpper();

            switch (formattedLoginType)
            {
                case USER_AUTH.KAKAO:
                    loginUrl = _authService.GetKakaoLoginUrl();
                    break;
                case USER_AUTH.NAVER:
                    loginUrl = _authService.GetNaverLoginUrl();
                    break;
                default:
                    return BadRequest("지원하지 않는 로그인 타입입니다.");
            }

            return Ok(new { loginUrl });
        }


        [SwaggerOperation(Summary = "로그인 - 카카오 (인증 후)")]
        [HttpPost("signin/kakao/callback")]
        public async Task<IActionResult> SignInKakaoCallback([FromQuery] string code)
        {
            var getTokenResponse = await _authService.GetKakaoToken(code);

            if (getTokenResponse.Success && getTokenResponse.Data != null)
            {
                var getKakaoUserInfo = await _authService.GetKakaoUserInfo(getTokenResponse.Data.ToString());

                if (getKakaoUserInfo.Success && getKakaoUserInfo.Data != null && getKakaoUserInfo.Data.Id != null)
                {
                    var isCompletedResponse = await _authService.IsCompleted(getKakaoUserInfo.Data.Id.ToString(), USER_AUTH.KAKAO);

                    if (isCompletedResponse.Success && isCompletedResponse.Data != null)
                    {
                        // 1. get user info and token
                        if (isCompletedResponse.Data.isCompleted)
                        {
                            var response = await _authService.SignInWithExternalProvider(USER_AUTH.KAKAO, getKakaoUserInfo.Data.Id.ToString());

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
            }

            return Ok(new { string.Empty });
        }

        [SwaggerOperation(Summary = "로그인 - 네이버 (인증 후)")]
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
                            var response = await _authService.SignInWithExternalProvider(USER_AUTH.NAVER, getUserInfo.Data.id);

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

        [SwaggerOperation(Summary = "회원가입 - 카카오 / 네이버 ")]
        [HttpPost]
        [Route("signup/external")]
        public async Task<IActionResult> SignUpWithExternalProvider([FromBody] SignUpWithExternalProviderRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.LoginType = request.LoginType.Length > 1
                            ? char.ToUpper(request.LoginType[0]) + request.LoginType.Substring(1).ToLower()
                            : request.LoginType.ToUpper();

            var response = await _authService.SignUpWithExternalProvider(request);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [SwaggerOperation(Summary = "회원가입 - 이메일")]
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUpEmail([FromBody] SignUpEmailRequestDto request)
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
