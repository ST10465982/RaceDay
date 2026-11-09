using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.Tests;

public class MediaControllerTests
{
    private static RaceDayDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    private static MediaController NewController(RaceDayDbContext context, int? userId = null, string? role = null)
    {
        // These authorization tests never call Azure; no cloud account is required.
        var config = new ConfigurationBuilder().Build();
        var controller = new MediaController(context, new AzureBlobStorageService(config));
        var http = new DefaultHttpContext { Session = new TestSession() };
        if (userId.HasValue) http.Session.SetInt32("UserID", userId.Value);
        if (role is not null) http.Session.SetString("Role", role);
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        return controller;
    }

    [Fact]
    public async Task ViewBanner_WithoutSession_Returns401()
    {
        using var context = NewContext();
        var controller = NewController(context);
        var result = await controller.GetBanner(1);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task UploadBanner_ParticipantRole_Returns403()
    {
        using var context = NewContext();
        var controller = NewController(context, 7, "Participant");
        var result = await controller.UploadBanner(1, null!);
        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
    }

    [Fact]
    public async Task UploadBanner_OtherOrganisersEvent_Returns403()
    {
        using var context = NewContext();
        var owner = new User
        {
            FirstName = "First", LastName = "Organiser", Email = "owner@raceday.test",
            Role = "Organiser", PasswordHash = "test-hash"
        };
        context.Users.Add(owner);
        await context.SaveChangesAsync();
        var race = new RaceEvent
        {
            OrganiserID = owner.UserID, EventName = "Demo Race",
            EventDate = DateTime.Today.AddDays(7), StartTime = new TimeSpan(8, 0, 0),
            Location = "Cape Town", DistanceKm = 10, EventType = "Run",
            MaximumParticipants = 25, Status = "Open"
        };
        context.Events.Add(race);
        await context.SaveChangesAsync();

        var controller = NewController(context, owner.UserID + 99, "Organiser");
        var result = await controller.UploadBanner(race.EventID, null!);
        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
    }
}
