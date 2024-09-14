using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewFace.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace NewFace.Controllers
{
    [AllowAnonymous]
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService)
        {
            _authService = authService;
        }

        [SwaggerOperation(Summary = "메인 페이지")]
        [HttpGet]
        public IActionResult Index()
        {
            var userId = _authService.GetUserIdFromToken();

            var response = new
            {

            };

            return Ok(response);
        }


    }
}
