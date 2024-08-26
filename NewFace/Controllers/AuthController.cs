using Microsoft.AspNetCore.Mvc;
using NewFace.Common.Constants;
using NewFace.DTOs.Auth;
using NewFace.Services;

namespace NewFace.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.Login(request);

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

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.Register(request);

            if (!response.Success)
            {
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("sendOTP")]
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


        [HttpPost]
        [Route("verifyOTP")]
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
