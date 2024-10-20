using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> SendOTP(int userId, string phone)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.SendOTP(userId, phone);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);

        }


        [SwaggerOperation(Summary = "핸드폰 OTP 번호 확인")]
        [HttpPost]
        [Route("verify-otp")]
        public async Task<IActionResult> VerifyOTP(int userId, string inputOTP)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.VerifyOTP(userId, inputOTP);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

    }

}
