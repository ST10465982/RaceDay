using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // CAPTURE RESULT
        // POST: /api/enrollments/{enrollmentId}/result
        // Organiser only
        // ============================================================

        [HttpPost("api/enrollments/{enrollmentId}/result")]
        public async Task<IActionResult> CreateResult(
            int enrollmentId,
            ResultRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to capture a result."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can capture race results."
                });
            }

            if (request.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "FinishTime must be greater than zero."
                });
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.RaceEvent)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentID == enrollmentId);

            if (enrollment == null)
            {
                return NotFound(new
                {
                    message = "Enrollment does not exist."
                });
            }

            if (enrollment.RaceEvent.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You may only capture results for events that you created."
                });
            }

            if (enrollment.EnrollmentStatus == "Cancelled")
            {
                return BadRequest(new
                {
                    message = "A result cannot be recorded for a cancelled enrollment."
                });
            }

            if (enrollment.Result != null)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrollment."
                });
            }

            var result = new Result
            {
                EnrollmentID = enrollmentId,
                FinishTime = request.FinishTime,
                OverallPosition = request.OverallPosition,
                CategoryPosition = request.CategoryPosition,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);

            // Once a result is captured the enrollment is complete.
            enrollment.EnrollmentStatus = "Completed";

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result captured successfully.",
                resultID = result.ResultID,
                enrollmentID = result.EnrollmentID
            });
        }


        // ============================================================
        // UPDATE RESULT
        // PUT: /api/results/{id}
        // Organiser only
        // ============================================================

        [HttpPut("api/results/{id}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            ResultRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to update a result."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can update race results."
                });
            }

            if (request.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "FinishTime must be greater than zero."
                });
            }

            var result = await _context.Results
                .Include(r => r.Enrollment)
                .ThenInclude(e => e.RaceEvent)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result does not exist."
                });
            }

            if (result.Enrollment.RaceEvent.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You may only update results for events that you created."
                });
            }

            result.FinishTime = request.FinishTime;
            result.OverallPosition = request.OverallPosition;
            result.CategoryPosition = request.CategoryPosition;
            result.RecordedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully."
            });
        }


        // ============================================================
        // GET RESULTS FOR AN EVENT
        // GET: /api/events/{eventId}/results
        // Available to any logged-in user
        // ============================================================

        [HttpGet("api/events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view event results."
                });
            }

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event does not exist."
                });
            }

            var results = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrollment.EventID == eventId)
                .Select(r => new
                {
                    resultID = r.ResultID,
                    enrollmentID = r.EnrollmentID,

                    participantID =
                        r.Enrollment.ParticipantID,

                    participantName =
                        r.Enrollment.Participant.FirstName +
                        " " +
                        r.Enrollment.Participant.LastName,

                    categoryID =
                        r.Enrollment.CategoryID,

                    categoryName =
                        r.Enrollment.Category.CategoryName,

                    finishTime = r.FinishTime,
                    overallPosition = r.OverallPosition,
                    categoryPosition = r.CategoryPosition,
                    recordedAt = r.RecordedAt
                })
                .OrderBy(r => r.overallPosition)
                .ToListAsync();

            return Ok(results);
        }


        // ============================================================
        // GET LOGGED-IN PARTICIPANT'S RESULTS
        // GET: /api/results/me
        // Participant only
        // ============================================================

        [HttpGet("api/results/me")]
        public async Task<IActionResult> GetMyResults()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view your results."
                });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can view personal results."
                });
            }

            var results = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.RaceEvent)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Category)
                .Where(r =>
                    r.Enrollment.ParticipantID == userID.Value)
                .Select(r => new
                {
                    resultID = r.ResultID,

                    eventID =
                        r.Enrollment.EventID,

                    eventName =
                        r.Enrollment.RaceEvent.EventName,

                    eventDate =
                        r.Enrollment.RaceEvent.EventDate,

                    eventType =
                        r.Enrollment.RaceEvent.EventType,

                    location =
                        r.Enrollment.RaceEvent.Location,

                    categoryID =
                        r.Enrollment.CategoryID,

                    categoryName =
                        r.Enrollment.Category.CategoryName,

                    finishTime =
                        r.FinishTime,

                    overallPosition =
                        r.OverallPosition,

                    categoryPosition =
                        r.CategoryPosition,

                    recordedAt =
                        r.RecordedAt
                })
                .OrderByDescending(r => r.eventDate)
                .ToListAsync();

            return Ok(results);
        }
    }
}