using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.Tests;

public class ReEnrollmentTests
{
    [Fact]
    public async Task CancelledParticipant_CanReEnterWithoutDuplicateDatabaseRow()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new RaceDayDbContext(options);
        var organiser = new User
        {
            FirstName = "Test", LastName = "Organiser", Email = "organiser@raceday.test",
            PasswordHash = "not-used-in-this-test", Role = "Organiser"
        };
        var participant = new User
        {
            FirstName = "Test", LastName = "Participant", Email = "participant@raceday.test",
            PasswordHash = "not-used-in-this-test", Role = "Participant"
        };
        db.Users.AddRange(organiser, participant);
        await db.SaveChangesAsync();
        var race = new RaceEvent
        {
            OrganiserID = organiser.UserID, EventName = "Test event",
            EventDate = DateTime.Today.AddDays(10), StartTime = new TimeSpan(7, 0, 0),
            Location = "Johannesburg", DistanceKm = 10, EventType = "Run",
            MaximumParticipants = 50, Status = "Open"
        };
        db.Events.Add(race);
        await db.SaveChangesAsync();
        var category = new Category
        {
            EventID = race.EventID, CategoryName = "10km", CategoryType = "Distance", DistanceKm = 10
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        var cancelled = new Enrollment
        {
            EventID = race.EventID, ParticipantID = participant.UserID,
            CategoryID = category.CategoryID, EnrollmentStatus = "Cancelled"
        };
        db.Enrollments.Add(cancelled);
        await db.SaveChangesAsync();

        var controller = new EnrollmentsController(db);
        var http = new DefaultHttpContext { Session = new TestSession() };
        http.Session.SetInt32("UserID", participant.UserID);
        http.Session.SetString("Role", "Participant");
        controller.ControllerContext = new ControllerContext { HttpContext = http };

        var result = await controller.EnrollInEvent(
            race.EventID, new EnrollmentRequest { CategoryID = category.CategoryID });

        var created = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Single(db.Enrollments);
        Assert.Equal("Active", cancelled.EnrollmentStatus);
    }
}
