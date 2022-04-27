using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.DBContexts;
using UserService.DTO;
using UserService.Models;
using RabbitMQLibrary;
using BC = BCrypt.Net.BCrypt;

namespace UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Database context for users, this is used to make calls to the database.
        /// </summary>
        private readonly UserServiceDatabaseContext _dbContext;

        /// <summary>
        /// Constructer is used for receiving the database context at the creation of the UserController.
        /// </summary>
        /// <param name="dbContext">Context of the database</param>
        public UserController(UserServiceDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (registerModel == null)
            {
                return BadRequest("NO_DATA");
            }

            if (string.IsNullOrWhiteSpace(registerModel.Username))
            {
                return BadRequest("NO_USERNAME");
            }

            if (string.IsNullOrWhiteSpace(registerModel.Password))
            {
                return BadRequest("NO_PASSWORD");
            }

            if (string.IsNullOrWhiteSpace(registerModel.Email))
            {
                return BadRequest("INCORRECT_EMAIL");
            }

            if (string.IsNullOrWhiteSpace(registerModel.Name))
            {
                return BadRequest("NO_NAME");
            }

            if (await _dbContext.Users.AnyAsync(x => x.Username == registerModel.Username))
            {
                return BadRequest("USERNAME_TAKEN");
            }

            if (await _dbContext.Users.AnyAsync(x => x.Email == registerModel.Email))
            {
                return BadRequest("EMAIL_TAKEN");
            }

            Guid? verifyToken = Guid.Empty;
            do
            {
                verifyToken = Guid.NewGuid();
            } while (await _dbContext.Users.AnyAsync(x => x.VerifyEmailToken == verifyToken));

            var role = await _dbContext.Roles.FindAsync(1);
            var user = new User()
            {
                Username = registerModel.Username,
                Password = BC.HashPassword(registerModel.Password, BC.GenerateSalt(12)),
                Email = registerModel.Email,
                VerifyEmailToken = verifyToken,
                Role = role
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }


        [HttpGet("testuser")]
        public async Task<IActionResult> GetTestUser()
        {
            User user = new User();
            user.Id = 1;
            user.Username = "this is a test user";
            user.Created = DateTime.Now;

            return Ok(user);
        }
        /// <summary>
        /// Get all the Users from the database.
        /// </summary>
        /// <returns>All Users in Db</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _dbContext.Users.ToListAsync();
            List<User> users = new List<User>();
            foreach (var item in result)
            {
                users.Add(new User() { Id = item.Id });
            }
            
            return Ok(users);
        }

        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser(LoginModel loginModel)
        {
            if (loginModel == null)
            {
                return BadRequest();
            }
            if (string.IsNullOrWhiteSpace(loginModel.Username))
            {
                return BadRequest(ModelState);
            }

            var user = new User()
            {
                Username = loginModel.Username
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

       
    }
}