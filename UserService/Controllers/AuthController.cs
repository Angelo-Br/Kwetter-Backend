using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using UserService.DBContexts;
using UserService.DTO;
using UserService.Helpers;
using BC = BCrypt.Net.BCrypt;

namespace UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Database context for users, this is used to make calls to the database.
        /// </summary>
        private readonly UserServiceDatabaseContext _dbContext;

        public AuthController(UserServiceDatabaseContext context)
        {
            _dbContext = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.Username))
            {
                return BadRequest("MISSING_USERNAME");
            }

            if (string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest("MISSING_PASSWORD");
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == loginModel.Username);
            if (user != null && BC.Verify(loginModel.Password, user.Password, false, BCrypt.Net.HashType.SHA384))
            {
                // SWAP THIS BACK TO != NULL ONCE THE EMAIL VERIFICATION IS IMPLEMENTED
                if (user.VerifyEmailToken == null)
                {
                    // Maybe resent mail
                    return Unauthorized("UNVERIFIED_ACCOUNT");
                }

                var token = new TokenBuilder().BuildToken(user.Id);
                var serializedToken = JsonSerializer.Serialize(token);
                return Ok(serializedToken);
            }

            else
            {
                return BadRequest("INCORRECT_DETAILS");
            }
            
        }

        [HttpPost("verifyemail")]
        public async Task<IActionResult> VerifyToken(VerifyEmailTokenModel verifyEmailTokenModel)
        {
            if (verifyEmailTokenModel == null)
            {
                return BadRequest();
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == verifyEmailTokenModel.Username && x.Password == verifyEmailTokenModel.Password);

            if (user == null)
            {
                return BadRequest("INCORRECT_INFO");
            }

            if (user.VerifyEmailToken == null)
            {
                return BadRequest("ALREADY_VERIFIED");
            }

            if (user.VerifyEmailToken != verifyEmailTokenModel.VerifyEmailToken)
            {
                return BadRequest("INCORRECT_TOKEN");
            }

            user.VerifyEmailToken = null;
            await _dbContext.SaveChangesAsync();
            return await Login(verifyEmailTokenModel);
        }

        [HttpGet("loggeduser")]
        [Authorize]
        public async Task<IActionResult> GetUserLog()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(userId);
        }
    }
}
