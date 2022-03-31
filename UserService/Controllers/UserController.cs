using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.DBContexts;
using UserService.DTO;
using UserService.Models;
using RabbitMQLibrary;

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

        /// <summary>
        /// Get all the Users from the database
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