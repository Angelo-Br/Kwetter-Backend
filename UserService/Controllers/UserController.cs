using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.DBContexts;
using UserService.DTO;
using UserService.Models;
using RabbitMQLibrary;
using BC = BCrypt.Net.BCrypt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UserService.Helpers;

namespace UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Logger for all logging events
        /// </summary>
        private readonly ILogger<UserController> _logger;
        /// <summary>
        /// Database context for users, this is used to make calls to the database.
        /// </summary>
        private readonly UserServiceDatabaseContext _dbContext;
        /// <summary>
        /// Rabbitmq producer interface to interact with rabbitMQ
        /// </summary>
        private readonly IMessageProducer _producer;
        private readonly UserServiceHelper _helper;

        /// <summary>
        /// Constructer is used for receiving the database context at the creation of the UserController.
        /// </summary>
        /// <param name="dbContext">Context of the database</param>
        public UserController(ILogger<UserController> logger, IMessageProducer producer, UserServiceDatabaseContext dbContext)
        {
            _helper = new UserServiceHelper();
            _producer = producer;
            _logger = logger;
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

            _producer.PublishMessageAsync(RoutingKeyType.UserCreated, _helper.UserToKweetServiceUserDTO(user));

            return Ok();
        }


        [HttpGet("testuser")]
        [Authorize]
        public async Task<IActionResult> GetTestUser()
        {
            User user = new User();
            user.Id = 1;
            user.Username = "this is a test user";
            user.Created = DateTime.Now;

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            LoggedUserModel model = new LoggedUserModel()
            {
                token = userId,
            };
            _logger.LogInformation("User with token {userId} has been tested");

            return Ok(model);
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

        [HttpPost("changeusername")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameModel changeUsernameModel)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userId));
            if (user == null)
            {
                return BadRequest("User doesnt exist");
            }
            var existingUsername = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == changeUsernameModel.NewUserName);
            if (existingUsername != null)
            {
                return BadRequest("username is already taken");
            }
            if (BC.Verify(changeUsernameModel.Password, user.Password, false, BCrypt.Net.HashType.SHA384))
            {
                string oldUsername = user.Username;
                user.Username = changeUsernameModel.NewUserName;
                await _dbContext.SaveChangesAsync();

                //Rabbitmq calls
                _producer.PublishMessageAsync(RoutingKeyType.UsernameUpdated, _helper.UserToUsernameUpdatedDTO(Convert.ToInt32(userId), changeUsernameModel.NewUserName, oldUsername));
                return Ok("Changed username to " + changeUsernameModel.NewUserName);
            }
            else
            {
                return BadRequest("Password doesnt match");
            }
           
        }

        [HttpPost("deleteuser")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(DeleteUserModel deleteUserModel)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(userId));
            if (user == null)
            {
                return BadRequest("User doesnt exist");
            }
            if (BC.Verify(deleteUserModel.Password, user.Password, false, BCrypt.Net.HashType.SHA384))
            {
                _dbContext.Remove(user);
                _dbContext.SaveChanges();

                _producer.PublishMessageAsync(RoutingKeyType.UserDeleted, _helper.UserToKweetServiceUserDTO(user));
                return Ok("Deleted your account");
            }
            else
            {
                return BadRequest("Password or username didnt match");
            }
        }

        //OLD TEST CODE
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