
using Commander.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Commander.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IJwtAuthenticationManager jwtAuthenticationManager;

        public LoginController(IJwtAuthenticationManager jwtAuthenticationManager) {
            this.jwtAuthenticationManager = jwtAuthenticationManager;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserCred userCred)
        {
            string token = jwtAuthenticationManager.Authenticate
                (userCred.UserName, userCred.Password);

            if (token != null)
            {
                return Ok(token);
            }

            return Unauthorized();            
        }
    }
}