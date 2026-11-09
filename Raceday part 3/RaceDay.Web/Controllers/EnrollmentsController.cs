using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;
public class EnrollmentsController : Controller
{
    private readonly RaceDayApiService _api;
    public EnrollmentsController(RaceDayApiService api) => _api = api;

    [RequireRole("Participant")]
    public async Task<IActionResult> My()
    {
        var result = await _api.GetAsync<List<EnrollmentDto>>("api/enrollments/me");
        if (!result.Success) TempData["Error"] = result.Error;
        return View(result.Data ?? new());
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Participant")]
    public async Task<IActionResult> Enter(int eventId, int categoryID)
    {
        var result = await _api.PostAsync<MessageDto>($"api/events/{eventId}/enrollments", new { categoryID });
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? "You're enrolled! Check My Entries." : result.Error;
        return result.Success ? RedirectToAction(nameof(My))
            : RedirectToAction("Details", "Events", new { id = eventId });
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _api.DeleteAsync<MessageDto>($"api/enrollments/{id}");
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Entry cancelled." : result.Error;
        return RedirectToAction(nameof(My));
    }

    [RequireRole("Organiser")]
    public async Task<IActionResult> Event(int eventId)
    {
        var result = await _api.GetAsync<List<EnrollmentDto>>($"api/events/{eventId}/enrollments");
        if (!result.Success) TempData["Error"] = result.Error;
        ViewBag.EventID = eventId;
        return View(result.Data ?? new());
    }
}
