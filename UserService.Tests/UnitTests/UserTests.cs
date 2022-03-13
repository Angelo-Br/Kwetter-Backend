using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Controllers;
using UserService.DBContexts;
using UserService.Models;
using Xunit;

namespace UserService.Tests
{
    public class UserTests: IDisposable
    {
        private readonly UserController _controller;
        private readonly UserServiceDatabaseContext _dbContext;

        public UserTests()
        {
            var options = new DbContextOptionsBuilder<UserServiceDatabaseContext>().UseInMemoryDatabase(databaseName: "InMemoryImageDb").Options;

            _dbContext = new UserServiceDatabaseContext(options);
            _controller = new UserController(_dbContext);
            SeedUserInMemoryDatabaseWithData(_dbContext);
        }

        [Fact]
        public async Task GetUsers_WhenCalled_ReturnListOfUsers()
        {
            var result = await _controller.GetUsers();

            var objectresult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<IEnumerable<User>>(objectresult.Value);

            Assert.Equal(3, users.Count());
            Assert.Equal(1, users.ElementAt(0).Id);
            Assert.Equal(2, users.ElementAt(1).Id);
            Assert.Equal(5, users.ElementAt(2).Id);
        }

        private static void SeedUserInMemoryDatabaseWithData(UserServiceDatabaseContext context)
        {
            var data = new List<User>
                {
                    new User { Id = 1 },
                    new User { Id = 2 },
                    new User { Id = 5 }
                };
            context.Users.AddRange(data);
            context.SaveChanges();
        }

        public void Dispose()
        {
            _dbContext.Users.RemoveRange(_dbContext.Users.ToList());
            _dbContext.SaveChanges();
        }
    }
}