using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
public class MediaController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly AzureBlobStorageService _blobs;
    public MediaController(RaceDayDbContext db, AzureBlobStorageService blobs)
    {
        _db = db;
        _blobs = blobs;
    }

    [HttpGet("api/media/events/{id:int}")]
    public async Task<IActionResult> GetBanner(int id)
    {
        if (HttpContext.Session.GetInt32("UserID") is null) return Unauthorized(new { message = "Sign in first." });
        var race = await _db.Events.FindAsync(id);
        if (race is null) return NotFound(new { message = "Event not found." });
        try { return Ok(new { url = _blobs.GetReadUrl(race.BannerImageUrl) }); }
        catch (Exception ex) when (ex is InvalidOperationException or UriFormatException)
        { return StatusCode(503, new { message = "Image storage is not ready." }); }
    }

    [HttpPost("api/media/events/{id:int}/banner")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var user = HttpContext.Session.GetInt32("UserID");
        if (user is null) return Unauthorized(new { message = "Sign in first." });
        if (HttpContext.Session.GetString("Role") != "Organiser") return StatusCode(403, new { message = "Organisers only." });
        var race = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (race is null) return NotFound(new { message = "Event not found." });
        if (race.OrganiserID != user.Value) return StatusCode(403, new { message = "Not your event." });
        try
        {
            race.BannerImageUrl = await _blobs.UploadAsync(file, "event-banners");
            await _db.SaveChangesAsync();
            return Ok(new { url = _blobs.GetReadUrl(race.BannerImageUrl) });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) when (ex is RequestFailedException or InvalidOperationException)
        { return StatusCode(503, new { message = "Azure image upload is unavailable. Check storage configuration." }); }
    }

    [HttpGet("api/media/profile")]
    public async Task<IActionResult> GetProfilePhoto()
    {
        var user = HttpContext.Session.GetInt32("UserID");
        if (user is null) return Unauthorized(new { message = "Sign in first." });
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserID == user.Value);
        try { return Ok(new { url = _blobs.GetReadUrl(profile?.ProfilePictureUrl) }); }
        catch (Exception ex) when (ex is InvalidOperationException or UriFormatException)
        { return StatusCode(503, new { message = "Image storage is not ready." }); }
    }

    [HttpPost("api/media/profile")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        var user = HttpContext.Session.GetInt32("UserID");
        if (user is null) return Unauthorized(new { message = "Sign in first." });
        try
        {
            var savedUri = await _blobs.UploadAsync(file, "profile-photos");
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserID == user.Value);
            if (profile is null)
            {
                profile = new UserProfile { UserID = user.Value };
                _db.UserProfiles.Add(profile);
            }
            profile.ProfilePictureUrl = savedUri;
            await _db.SaveChangesAsync();
            return Ok(new { url = _blobs.GetReadUrl(savedUri) });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) when (ex is RequestFailedException or InvalidOperationException)
        { return StatusCode(503, new { message = "Azure image upload is unavailable. Check storage configuration." }); }
    }
}
