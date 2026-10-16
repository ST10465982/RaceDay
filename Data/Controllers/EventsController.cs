using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET ALL EVENTS
        // GET: /api/events
        // Available to any logged-in user
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view events."
                });
            }

            var events = await _context.Events
                .Include(e => e.Organiser)
                .Select(e => new
                {
                    eventID = e.EventID,
                    eventName = e.EventName,
                    description = e.Description,
                    eventDate = e.EventDate,
                    startTime = e.StartTime,
                    location = e.Location,
                    distanceKm = e.DistanceKm,
                    eventType = e.EventType,
                    maximumParticipants = e.MaximumParticipants,
                    status = e.Status,
                    organiserID = e.OrganiserID,
                    organiserName = e.Organiser.FirstName + " " + e.Organiser.LastName,
                    createdAt = e.CreatedAt
                })
                .OrderBy(e => e.eventDate)
                .ToListAsync();

            return Ok(events);
        }

        // ============================================================
        // GET EVENT BY ID
        // GET: /api/events/{id}
        // Available to any logged-in user
        // ============================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view event details."
                });
            }

            var raceEvent = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Where(e => e.EventID == id)
                .Select(e => new
                {
                    eventID = e.EventID,
                    eventName = e.EventName,
                    description = e.Description,
                    eventDate = e.EventDate,
                    startTime = e.StartTime,
                    location = e.Location,
                    distanceKm = e.DistanceKm,
                    eventType = e.EventType,
                    maximumParticipants = e.MaximumParticipants,
                    status = e.Status,
                    organiserID = e.OrganiserID,
                    organiserName = e.Organiser.FirstName + " " + e.Organiser.LastName,
                    createdAt = e.CreatedAt,
                    categories = e.Categories.Select(c => new
                    {
                        categoryID = c.CategoryID,
                        categoryName = c.CategoryName,
                        categoryType = c.CategoryType,
                        minimumAge = c.MinimumAge,
                        maximumAge = c.MaximumAge,
                        distanceKm = c.DistanceKm,
                        description = c.Description
                    })
                })
                .FirstOrDefaultAsync();

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event does not exist."
                });
            }

            return Ok(raceEvent);
        }

        // ============================================================
        // CREATE EVENT
        // POST: /api/events
        // Organiser only
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to create an event."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can create events."
                });
            }

            string eventType = request.EventType.Trim();

            if (eventType != "Run" &&
                eventType != "Walk" &&
                eventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "EventType must be Run, Walk or Cycle."
                });
            }

            string status = request.Status.Trim();

            if (status != "Open" &&
                status != "Closed" &&
                status != "Completed" &&
                status != "Cancelled")
            {
                return BadRequest(new
                {
                    message = "Status must be Open, Closed, Completed or Cancelled."
                });
            }

            var raceEvent = new RaceEvent
            {
                OrganiserID = userID.Value,
                EventName = request.EventName.Trim(),
                Description = request.Description?.Trim(),
                EventDate = request.EventDate,
                StartTime = request.StartTime,
                Location = request.Location.Trim(),
                DistanceKm = request.DistanceKm,
                EventType = eventType,
                MaximumParticipants = request.MaximumParticipants,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(raceEvent);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Event created successfully.",
                eventID = raceEvent.EventID
            });
        }

        // ============================================================
        // UPDATE EVENT
        // PUT: /api/events/{id}
        // Organiser only and must own event
        // ============================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            EventRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to update an event."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can update events."
                });
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

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
                    message = "You may only update events that you created."
                });
            }

            string eventType = request.EventType.Trim();

            if (eventType != "Run" &&
                eventType != "Walk" &&
                eventType != "Cycle")
            {
                return BadRequest(new
                {
                    message = "EventType must be Run, Walk or Cycle."
                });
            }

            string status = request.Status.Trim();

            if (status != "Open" &&
                status != "Closed" &&
                status != "Completed" &&
                status != "Cancelled")
            {
                return BadRequest(new
                {
                    message = "Status must be Open, Closed, Completed or Cancelled."
                });
            }

            raceEvent.EventName = request.EventName.Trim();
            raceEvent.Description = request.Description?.Trim();
            raceEvent.EventDate = request.EventDate;
            raceEvent.StartTime = request.StartTime;
            raceEvent.Location = request.Location.Trim();
            raceEvent.DistanceKm = request.DistanceKm;
            raceEvent.EventType = eventType;
            raceEvent.MaximumParticipants = request.MaximumParticipants;
            raceEvent.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully."
            });
        }

        // ============================================================
        // DELETE EVENT
        // DELETE: /api/events/{id}
        // Organiser only and must own event
        // ============================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to delete an event."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can delete events."
                });
            }

            var raceEvent = await _context.Events
                .Include(e => e.Enrollments)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

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
                    message = "You may only delete events that you created."
                });
            }

            if (raceEvent.Enrollments.Any())
            {
                return Conflict(new
                {
                    message = "This event cannot be deleted because participants are already enrolled."
                });
            }

            _context.Events.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}