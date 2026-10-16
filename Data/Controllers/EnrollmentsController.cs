using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrollmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // ENROL IN EVENT
        // POST: /api/events/{eventId}/enrollments
        // Participant only
        // ============================================================

        [HttpPost("api/events/{eventId}/enrollments")]
        public async Task<IActionResult> EnrollInEvent(
            int eventId,
            EnrollmentRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to enrol in an event."
                });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can enrol in events."
                });
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event does not exist."
                });
            }

            if (raceEvent.Status != "Open")
            {
                return BadRequest(new
                {
                    message = "This event is not currently open for enrolment."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == request.CategoryID &&
                    c.EventID == eventId);

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "The selected category does not belong to this event."
                });
            }

            bool alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.ParticipantID == userID.Value &&
                    e.EnrollmentStatus != "Cancelled");

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            int currentParticipants = await _context.Enrollments
                .CountAsync(e =>
                    e.EventID == eventId &&
                    e.EnrollmentStatus != "Cancelled");

            if (currentParticipants >= raceEvent.MaximumParticipants)
            {
                return Conflict(new
                {
                    message = "This event has reached its maximum participant limit."
                });
            }

            var enrollment = new Enrollment
            {
                EventID = eventId,
                ParticipantID = userID.Value,
                CategoryID = request.CategoryID,
                EnrollmentDate = DateTime.UtcNow,
                EnrollmentStatus = "Active"
            };

            _context.Enrollments.Add(enrollment);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrollment created successfully.",
                enrollmentID = enrollment.EnrollmentID,
                eventID = enrollment.EventID,
                categoryID = enrollment.CategoryID
            });
        }


        // ============================================================
        // VIEW OWN ENROLLMENTS
        // GET: /api/enrollments/me
        // Participant only
        // ============================================================

        [HttpGet("api/enrollments/me")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view your enrollments."
                });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can view personal enrollments."
                });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.ParticipantID == userID.Value)
                .Include(e => e.RaceEvent)
                .Include(e => e.Category)
                .Select(e => new
                {
                    enrollmentID = e.EnrollmentID,
                    eventID = e.EventID,
                    eventName = e.RaceEvent.EventName,
                    eventDate = e.RaceEvent.EventDate,
                    location = e.RaceEvent.Location,
                    categoryID = e.CategoryID,
                    categoryName = e.Category.CategoryName,
                    enrollmentDate = e.EnrollmentDate,
                    enrollmentStatus = e.EnrollmentStatus
                })
                .OrderByDescending(e => e.enrollmentDate)
                .ToListAsync();

            return Ok(enrollments);
        }


        // ============================================================
        // VIEW ENROLLMENTS FOR AN EVENT
        // GET: /api/events/{eventId}/enrollments
        // Organiser only and must own the event
        // ============================================================

        [HttpGet("api/events/{eventId}/enrollments")]
        public async Task<IActionResult> GetEventEnrollments(int eventId)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view event enrollments."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can view event enrollments."
                });
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event does not exist."
                });
            }

            if (raceEvent.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You may only view enrollments for events that you created."
                });
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.EventID == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Select(e => new
                {
                    enrollmentID = e.EnrollmentID,
                    participantID = e.ParticipantID,
                    participantName =
                        e.Participant.FirstName + " " +
                        e.Participant.LastName,
                    participantEmail = e.Participant.Email,
                    categoryID = e.CategoryID,
                    categoryName = e.Category.CategoryName,
                    enrollmentDate = e.EnrollmentDate,
                    enrollmentStatus = e.EnrollmentStatus
                })
                .OrderBy(e => e.participantName)
                .ToListAsync();

            return Ok(enrollments);
        }


        // ============================================================
        // CANCEL OWN ENROLLMENT
        // DELETE: /api/enrollments/{id}
        // Participant only
        // ============================================================

        [HttpDelete("api/enrollments/{id}")]
        public async Task<IActionResult> CancelEnrollment(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to cancel an enrollment."
                });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can cancel their own enrollments."
                });
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.RaceEvent)
                .FirstOrDefaultAsync(e => e.EnrollmentID == id);

            if (enrollment == null)
            {
                return NotFound(new
                {
                    message = "Enrollment does not exist."
                });
            }

            if (enrollment.ParticipantID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You may only cancel your own enrollment."
                });
            }

            if (enrollment.EnrollmentStatus == "Cancelled")
            {
                return BadRequest(new
                {
                    message = "This enrollment has already been cancelled."
                });
            }

            if (enrollment.EnrollmentStatus == "Completed")
            {
                return BadRequest(new
                {
                    message = "A completed enrollment cannot be cancelled."
                });
            }

            enrollment.EnrollmentStatus = "Cancelled";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrollment cancelled successfully."
            });
        }
    }
}