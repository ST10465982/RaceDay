using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view your profile."
                });
            }

            var profile = await _context.UserProfiles
                .Where(p => p.UserID == userID.Value)
                .Select(p => new
                {
                    profileID = p.ProfileID,
                    userID = p.UserID,
                    phoneNumber = p.PhoneNumber,
                    dateOfBirth = p.DateOfBirth,
                    emergencyContactName = p.EmergencyContactName,
                    emergencyContactNumber = p.EmergencyContactNumber
                })
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Profile does not exist yet."
                });
            }

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> CreateOrUpdateProfile(
            ProfileRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to update your profile."
                });
            }

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserID == userID.Value);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserID = userID.Value,
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    DateOfBirth = request.DateOfBirth,
                    EmergencyContactName =
                        request.EmergencyContactName?.Trim(),
                    EmergencyContactNumber =
                        request.EmergencyContactNumber?.Trim()
                };

                _context.UserProfiles.Add(profile);

                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Profile created successfully.",
                    profileID = profile.ProfileID
                });
            }

            profile.PhoneNumber = request.PhoneNumber?.Trim();
            profile.DateOfBirth = request.DateOfBirth;
            profile.EmergencyContactName =
                request.EmergencyContactName?.Trim();
            profile.EmergencyContactNumber =
                request.EmergencyContactNumber?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                profileID = profile.ProfileID
            });
        }
    }
}