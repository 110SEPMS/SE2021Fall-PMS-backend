using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly AuthorizeService _authorizationService;
        private readonly UserService _userService;
        public AuthorizeController(IConfiguration configuration, PMSContext context, JwtHelper jwt)
        {
            _authorizationService = new AuthorizeService(context, configuration, jwt);
            _userService = new UserService(context);
        }

        [HttpPost("github")]
        public async Task<IActionResult> AuthenticateGithub(GithubLoginDto dto)
        {
            return Ok(await _authorizationService.AuthenticateGithub(dto));
        }

        [HttpPost("admin")]
        public IActionResult AuthenticateLocal(LocalAccountDto dto)
        {
            var result = _authorizationService.AuthenticateLocal(dto);
            if (result != null)
            {
                return Ok(result);
            }
            return ValidationProblem("Account or password error！");
        }

        [HttpPost]
        public IActionResult CreateAdmin(LocalAccountDto dto, [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName == "Development")
            {
                _userService.CreateUser(dto);
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult CheckAuthentucate()
        {
            return Ok(User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value);
        }
    }
}
