using JWT.Exeptions;
using JWT.RequestModels;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IWebUserService service;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(RequestModels.LoginRequestModel model)
        {
            try
            {
                return Ok(await service.LoginUserAsync(model, configuration));
            }
            catch (CustomException e)
            {
                return Unauthorized(e.Message);
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterUserRequestModel model)
        {
            try
            {
                await service.RegisterUserAsync(model);
                return Ok();
            }
            catch (CustomException e)
            { 
                return BadRequest(e.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RequestModels.RefreshTokenRequestModel model)
        {
            try
            {
                var results = await service.RefreshUserToken(model, configuration);
                return Ok(results);
            }
            catch (CustomException e)
            {
                return Unauthorized(e.Message);
            }
        }

        [HttpGet("protected-information")]
        [Authorize]
        public IActionResult GetProtectedInformation()
        {
            return Ok("Very protected informations. Top Secret");
        }
    }
}
