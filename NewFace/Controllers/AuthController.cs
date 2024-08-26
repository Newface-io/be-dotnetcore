using Microsoft.AspNetCore.Mvc;
using NewFace.Common.Constants;
using NewFace.DTOs.Auth;
using NewFace.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

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
        public async Task<IActionResult> SendOTP(string phone)
        {
            string accountSid = "ACef2c092db25101119d8c411863a4471a";
            string authToken = "69303da4b63a3a9a9c5466e5332bf0d8";

            TwilioClient.Init(accountSid, authToken);

            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString("D6");

            var message = MessageResource.Create(
                body: "NewFace 휴대폰 인증번호는 " + otp,
                from: new Twilio.Types.PhoneNumber("+17204427345"),
                to: new Twilio.Types.PhoneNumber("+821059601017")
            );

            if (message.Status == MessageResource.StatusEnum.Queued ||
                message.Status == MessageResource.StatusEnum.Sent ||
                message.Status == MessageResource.StatusEnum.Delivered)
            {
                return Ok(new 
                {
                    message = "SMS sent successfully",
                    sid = message.Sid,
                    status = message.Status.ToString()
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    error = "SMS sending failed",
                    status = message.Status.ToString(),
                    errorMessage = message.ErrorMessage
                });
            }

        }

    }

}
