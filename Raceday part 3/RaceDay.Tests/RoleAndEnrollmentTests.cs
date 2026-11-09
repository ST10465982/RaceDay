using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.Tests
{
    public class RoleAndEnrollmentTests
    {
        private RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private void SetSession(
            ControllerBase controller,
            int userID,
            string role)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            httpContext.Session.SetInt32("UserID", userID);
            httpContext.Session.SetString("Role", role);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Participant_CannotCreateEvent_Returns403()
        {
            using var context = CreateContext();

            var participant = new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant1@test.com",
                PasswordHash = "hash",
                Role = "Participant"
            };

            context.Users.Add(participant);
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            SetSession(
                controller,
                participant.UserID,
                "Participant");

            var request = new EventRequest
            {
                EventName = "Test Race",
                Description = "Test race event",
                EventDate = DateTime.UtcNow.AddDays(30),
                StartTime = new TimeSpan(7, 0, 0),
                Location = "Johannesburg",
                DistanceKm = 10,
                EventType = "Run",
                MaximumParticipants = 100,
                Status = "Open"
            };

            var result = await controller.CreateEvent(request);

            var objectResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanCreateEvent_Returns201()
        {
            using var context = CreateContext();

            var organiser = new User
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser1@test.com",
                PasswordHash = "hash",
                Role = "Organiser"
            };

            context.Users.Add(organiser);
            await context.SaveChangesAsync();

            var controller = new EventsController(context);

            SetSession(
                controller,
                organiser.UserID,
                "Organiser");

            var request = new EventRequest
            {
                EventName = "Johannesburg Test Run",
                Description = "A test running event",
                EventDate = DateTime.UtcNow.AddDays(30),
                StartTime = new TimeSpan(7, 0, 0),
                Location = "Johannesburg",
                DistanceKm = 10,
                EventType = "Run",
                MaximumParticipants = 100,
                Status = "Open"
            };

            var result = await controller.CreateEvent(request);

            var objectResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);
            Assert.Single(context.Events);
        }

        [Fact]
        public async Task Participant_CanEnrollInOpenEvent_Returns201()
        {
            using var context = CreateContext();

            var organiser = new User
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser2@test.com",
                PasswordHash = "hash",
                Role = "Organiser"
            };

            var participant = new User
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = "participant2@test.com",
                PasswordHash = "hash",
                Role = "Participant"
            };

            context.Users.AddRange(organiser, participant);
            await context.SaveChangesAsync();

            var raceEvent = new RaceEvent
            {
                OrganiserID = organiser.UserID,
                EventName = "Test 10K",
                EventDate = DateTime.UtcNow.AddDays(30),
                StartTime = new TimeSpan(7, 0, 0),
                Location = "Johannesburg",
                DistanceKm = 10,
                EventType = "Run",
                MaximumParticipants = 100,
                Status = "Open"
            };

            context.Events.Add(raceEvent);
            await context.SaveChangesAsync();

            var category = new Category
            {
                EventID = raceEvent.EventID,
                CategoryName = "10km",
                CategoryType = "Distance",
                DistanceKm = 10
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var controller =
                new EnrollmentsController(context);

            SetSession(
                controller,
                participant.UserID,
                "Participant");

            var request = new EnrollmentRequest
            {
                CategoryID = category.CategoryID
            };

            var result =
                await controller.EnrollInEvent(
                    raceEvent.EventID,
                    request);

            var objectResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);
            Assert.Single(context.Enrollments);
        }

        [Fact]
        public async Task Organiser_CannotEnrollInEvent_Returns403()
        {
            using var context = CreateContext();

            var organiser = new User
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = "organiser3@test.com",
                PasswordHash = "hash",
                Role = "Organiser"
            };

            context.Users.Add(organiser);
            await context.SaveChangesAsync();

            var controller =
                new EnrollmentsController(context);

            SetSession(
                controller,
                organiser.UserID,
                "Organiser");

            var request = new EnrollmentRequest
            {
                CategoryID = 1
            };

            var result =
                await controller.EnrollInEvent(
                    1,
                    request);

            var objectResult =
                Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, objectResult.StatusCode);
        }
    }
}
