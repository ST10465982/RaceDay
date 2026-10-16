using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task Register_ValidUser_Returns201Created()
        {
            using var context = CreateContext();

            var hasher = new PasswordHasher<User>();

            var controller = new AuthController(context, hasher);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser@test.com",
                Password = "Password123!",
                Role = "Organiser"
            };

            var result = await controller.Register(request);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409Conflict()
        {
            using var context = CreateContext();

            var existingUser = new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "duplicate@test.com",
                PasswordHash = "hash",
                Role = "Participant"
            };

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var controller = new AuthController(
                context,
                new PasswordHasher<User>());

            var request = new RegisterRequest
            {
                FirstName = "Second",
                LastName = "User",
                Email = "duplicate@test.com",
                Password = "Password123!",
                Role = "Participant"
            };

            var result = await controller.Register(request);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400BadRequest()
        {
            using var context = CreateContext();

            var controller = new AuthController(
                context,
                new PasswordHasher<User>());

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "Password123!",
                Role = "Administrator"
            };

            var result = await controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_CorrectCredentials_Returns200AndCreatesSession()
        {
            using var context = CreateContext();

            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant@test.com",
                Role = "Participant"
            };

            user.PasswordHash =
                hasher.HashPassword(user, "Password123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new AuthController(context, hasher);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var request = new LoginRequest
            {
                Email = "participant@test.com",
                Password = "Password123!"
            };

            var result = await controller.Login(request);

            Assert.IsType<OkObjectResult>(result);

            Assert.Equal(
                user.UserID,
                httpContext.Session.GetInt32("UserID"));

            Assert.Equal(
                "Participant",
                httpContext.Session.GetString("Role"));
        }

        [Fact]
        public async Task Login_IncorrectPassword_Returns401Unauthorized()
        {
            using var context = CreateContext();

            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "wrongpassword@test.com",
                Role = "Participant"
            };

            user.PasswordHash =
                hasher.HashPassword(user, "CorrectPassword123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new AuthController(context, hasher);

            var request = new LoginRequest
            {
                Email = "wrongpassword@test.com",
                Password = "WrongPassword"
            };

            var result = await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
