using Microsoft.AspNetCore.Mvc;
using NewFace.Data;

namespace NewFace.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

    }
}
