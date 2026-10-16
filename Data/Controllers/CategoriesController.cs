using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET CATEGORIES FOR A SPECIFIC EVENT
        // GET: /api/events/{eventId}/categories
        // Available to any logged-in user
        // ============================================================

        [HttpGet("api/events/{eventId}/categories")]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view event categories."
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

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new
                {
                    categoryID = c.CategoryID,
                    eventID = c.EventID,
                    categoryName = c.CategoryName,
                    categoryType = c.CategoryType,
                    minimumAge = c.MinimumAge,
                    maximumAge = c.MaximumAge,
                    distanceKm = c.DistanceKm,
                    description = c.Description
                })
                .OrderBy(c => c.categoryName)
                .ToListAsync();

            return Ok(categories);
        }

        // ============================================================
        // GET CATEGORY BY ID
        // GET: /api/categories/{id}
        // Available to any logged-in user
        // ============================================================

        [HttpGet("api/categories/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to view a category."
                });
            }

            var category = await _context.Categories
                .Where(c => c.CategoryID == id)
                .Select(c => new
                {
                    categoryID = c.CategoryID,
                    eventID = c.EventID,
                    categoryName = c.CategoryName,
                    categoryType = c.CategoryType,
                    minimumAge = c.MinimumAge,
                    maximumAge = c.MaximumAge,
                    distanceKm = c.DistanceKm,
                    description = c.Description
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category does not exist."
                });
            }

            return Ok(category);
        }

        // ============================================================
        // CREATE CATEGORY FOR EVENT
        // POST: /api/events/{eventId}/categories
        // Organiser only and must own the event
        // ============================================================

        [HttpPost("api/events/{eventId}/categories")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            CategoryRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to create a category."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can create categories."
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
                    message = "You may only create categories for events that you created."
                });
            }

            string categoryType = request.CategoryType.Trim();

            if (categoryType != "Age" &&
                categoryType != "Distance")
            {
                return BadRequest(new
                {
                    message = "CategoryType must be Age or Distance."
                });
            }

            if (categoryType == "Age")
            {
                if (request.MinimumAge == null &&
                    request.MaximumAge == null)
                {
                    return BadRequest(new
                    {
                        message = "An Age category must contain a minimum age, maximum age or both."
                    });
                }

                if (request.MinimumAge != null &&
                    request.MaximumAge != null &&
                    request.MinimumAge > request.MaximumAge)
                {
                    return BadRequest(new
                    {
                        message = "MinimumAge cannot be greater than MaximumAge."
                    });
                }
            }

            if (categoryType == "Distance" &&
                request.DistanceKm == null)
            {
                return BadRequest(new
                {
                    message = "A Distance category must contain DistanceKm."
                });
            }

            string categoryName = request.CategoryName.Trim();

            bool duplicateExists = await _context.Categories
                .AnyAsync(c =>
                    c.EventID == eventId &&
                    c.CategoryName.ToLower() ==
                    categoryName.ToLower());

            if (duplicateExists)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for the event."
                });
            }

            var category = new Category
            {
                EventID = eventId,
                CategoryName = categoryName,
                CategoryType = categoryType,
                MinimumAge = request.MinimumAge,
                MaximumAge = request.MaximumAge,
                DistanceKm = request.DistanceKm,
                Description = request.Description?.Trim()
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Category created successfully.",
                categoryID = category.CategoryID,
                eventID = category.EventID
            });
        }

        // ============================================================
        // UPDATE CATEGORY
        // PUT: /api/categories/{id}
        // Organiser only and must own related event
        // ============================================================

        [HttpPut("api/categories/{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            CategoryRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to update a category."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can update categories."
                });
            }

            var category = await _context.Categories
                .Include(c => c.RaceEvent)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category does not exist."
                });
            }

            if (category.RaceEvent.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You may only update categories belonging to your own events."
                });
            }

            string categoryType = request.CategoryType.Trim();

            if (categoryType != "Age" &&
                categoryType != "Distance")
            {
                return BadRequest(new
                {
                    message = "CategoryType must be Age or Distance."
                });
            }

            if (categoryType == "Age")
            {
                if (request.MinimumAge == null &&
                    request.MaximumAge == null)
                {
                    return BadRequest(new
                    {
                        message = "An Age category must contain a minimum age, maximum age or both."
                    });
                }

                if (request.MinimumAge != null &&
                    request.MaximumAge != null &&
                    request.MinimumAge > request.MaximumAge)
                {
                    return BadRequest(new
                    {
                        message = "MinimumAge cannot be greater than MaximumAge."
                    });
                }
            }

            if (categoryType == "Distance" &&
                request.DistanceKm == null)
            {
                return BadRequest(new
                {
                    message = "A Distance category must contain DistanceKm."
                });
            }

            string categoryName = request.CategoryName.Trim();

            bool duplicateExists = await _context.Categories
                .AnyAsync(c =>
                    c.EventID == category.EventID &&
                    c.CategoryID != id &&
                    c.CategoryName.ToLower() ==
                    categoryName.ToLower());

            if (duplicateExists)
            {
                return Conflict(new
                {
                    message = "A category with this name already exists for the event."
                });
            }

            category.CategoryName = categoryName;
            category.CategoryType = categoryType;
            category.MinimumAge = request.MinimumAge;
            category.MaximumAge = request.MaximumAge;
            category.DistanceKm = request.DistanceKm;
            category.Description = request.Description?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        // ============================================================
        // DELETE CATEGORY
        // DELETE: /api/categories/{id}
        // Organiser only and must own related event
        // ============================================================

        [HttpDelete("api/categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "You must be logged in to delete a category."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Only Organisers can delete categories."
                });
            }

            var category = await _context.Categories
                .Include(c => c.RaceEvent)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category does not exist."
                });
            }

            if (category.RaceEvent.OrganiserID != userID.Value)
            {
                return StatusCode(403, new
                {
                    message = "You may only delete categories belonging to your own events."
                });
            }

            if (category.Enrollments.Any())
            {
                return Conflict(new
                {
                    message = "This category cannot be deleted because participants are already enrolled in it."
                });
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}