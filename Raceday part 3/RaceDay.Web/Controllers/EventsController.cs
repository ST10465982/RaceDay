using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;
[RequireRole("Organiser", "Participant")]
public class EventsController : Controller
{
    private readonly RaceDayApiService _api;
    public EventsController(RaceDayApiService api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Explore(string? search, string? type)
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events");
        if (!result.Success) TempData["Error"] = result.Error;
        var events = (result.Data ?? new())
            .Where(e => e.EventDate.Date >= DateTime.Today && e.Status == "Open")
            .Where(e => string.IsNullOrWhiteSpace(search) ||
                        e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Location.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Where(e => string.IsNullOrWhiteSpace(type) || e.EventType == type)
            .OrderBy(e => e.EventDate).ToList();
        return View(new EventListViewModel { Events = events, Search = search, Type = type });
    }

    [HttpGet, RequireRole("Organiser")]
    public async Task<IActionResult> Index()
    {
        var response = await _api.GetAsync<List<EventDto>>("api/events");
        if (!response.Success) TempData["Error"] = response.Error;
        var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        return View((response.Data ?? new()).Where(e => e.OrganiserID == userId).OrderBy(e => e.EventDate).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var response = await _api.GetAsync<EventDto>($"api/events/{id}");
        if (!response.Success || response.Data is null)
        {
            TempData["Error"] = response.Error ?? "Event not found.";
            return RedirectToAction(nameof(Explore));
        }
        var banner = await _api.GetAsync<ImageUrlDto>($"api/media/events/{id}");
        response.Data.BannerUrl = banner.Data?.Url;
        return View(new EventDetailsViewModel { Event = response.Data });
    }

    [HttpGet, RequireRole("Organiser")]
    public IActionResult Create() => View(new EventFormViewModel());

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Organiser")]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PostAsync<MessageDto>("api/events", ToRequest(model));
        if (!response.Success || response.Data is null || response.Data.EventID <= 0)
        {
            ModelState.AddModelError("", response.Error ?? "Unable to create the event.");
            return View(model);
        }
        if (model.Banner is not null)
        {
            var upload = await _api.UploadAsync<ImageUrlDto>($"api/media/events/{response.Data.EventID}/banner", model.Banner);
            if (!upload.Success) TempData["Error"] = "Event created, but banner upload failed: " + upload.Error;
        }
        TempData["Success"] = "Event created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, RequireRole("Organiser")]
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _api.GetAsync<EventDto>($"api/events/{id}");
        if (!response.Success || response.Data is null) return NotFound();
        if (response.Data.OrganiserID != HttpContext.Session.GetInt32("UserID")) return Forbid();
        var e = response.Data;
        var image = await _api.GetAsync<ImageUrlDto>($"api/media/events/{id}");
        return View(new EventFormViewModel
        {
            EventID = id, EventName = e.EventName, Description = e.Description,
            EventDate = e.EventDate, StartTime = e.StartTime,
            Location = e.Location, DistanceKm = e.DistanceKm,
            EventType = e.EventType, MaximumParticipants = e.MaximumParticipants,
            Status = e.Status, ExistingBanner = image.Data?.Url
        });
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Organiser")]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (id != model.EventID) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PutAsync<MessageDto>($"api/events/{id}", ToRequest(model));
        if (!response.Success)
        {
            ModelState.AddModelError("", response.Error ?? "Update failed.");
            return View(model);
        }
        if (model.Banner is not null)
        {
            var upload = await _api.UploadAsync<ImageUrlDto>($"api/media/events/{id}/banner", model.Banner);
            if (!upload.Success) TempData["Error"] = "Event saved, but image upload failed: " + upload.Error;
        }
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _api.DeleteAsync<MessageDto>($"api/events/{id}");
        TempData[response.Success ? "Success" : "Error"] = response.Success ? "Event deleted." : response.Error;
        return RedirectToAction(nameof(Index));
    }

    private static object ToRequest(EventFormViewModel e) => new
    {
        e.EventName, e.Description, e.EventDate, e.StartTime, e.Location,
        e.DistanceKm, e.EventType, e.MaximumParticipants, e.Status
    };
}
