using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.DBContexts;
using UserService.Models;

namespace UserService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : Controller
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var result = await _dbContext.Users.ToListAsync();
            return Ok(result);
        }
    }
}