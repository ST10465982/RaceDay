using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;
public class ResultsController : Controller
{
    private readonly RaceDayApiService _api;
    public ResultsController(RaceDayApiService api) => _api = api;

    [RequireRole("Participant")]
    public async Task<IActionResult> My()
    {
        var result = await _api.GetAsync<List<ResultDto>>("api/results/me");
        if (!result.Success) TempData["Error"] = result.Error;
        var races = result.Data ?? new List<ResultDto>();
        foreach (var race in races)
        {
            var finishers = await _api.GetAsync<List<ResultDto>>($"api/events/{race.EventID}/results");
            race.TotalFinishers = finishers.Data?.Count ?? 0;
        }
        return View(races);
    }

    [RequireRole("Organiser")]
    public async Task<IActionResult> Event(int eventId)
    {
        var results = await _api.GetAsync<List<ResultDto>>($"api/events/{eventId}/results");
        var entries = await _api.GetAsync<List<EnrollmentDto>>($"api/events/{eventId}/enrollments");
        var race = await _api.GetAsync<EventDto>($"api/events/{eventId}");
        if (!results.Success) TempData["Error"] = results.Error;
        if (!entries.Success) TempData["Error"] = entries.Error;
        var vm = new ResultsPageViewModel
        {
            EventID = eventId, EventName = race.Data?.EventName ?? "Race results",
            Results = results.Data ?? new(), Enrollments = entries.Data ?? new()
        };
        return View(vm);
    }

    [HttpGet, RequireRole("Organiser")]
    public IActionResult Record(int eventId, int enrollmentId) => View(new ResultFormViewModel
    {
        EventID = eventId, EnrollmentID = enrollmentId
    });

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Organiser")]
    public async Task<IActionResult> Record(ResultFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.PostAsync<MessageDto>($"api/enrollments/{model.EnrollmentID}/result", ToRequest(model));
        if (!result.Success) { ModelState.AddModelError("", result.Error ?? "Unable to record result."); return View(model); }
        TempData["Success"] = "Result recorded.";
        return RedirectToAction(nameof(Event), new { eventId = model.EventID });
    }

    [HttpGet, RequireRole("Organiser")]
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        var list = await _api.GetAsync<List<ResultDto>>($"api/events/{eventId}/results");
        var result = list.Data?.FirstOrDefault(r => r.ResultID == id);
        if (result is null) return NotFound();
        return View(new ResultFormViewModel
        {
            ResultID = id, EnrollmentID = result.EnrollmentID, EventID = eventId,
            FinishTime = result.FinishTime, OverallPosition = result.OverallPosition,
            CategoryPosition = result.CategoryPosition
        });
    }

    [HttpPost, ValidateAntiForgeryToken, RequireRole("Organiser")]
    public async Task<IActionResult> Edit(int id, ResultFormViewModel model)
    {
        if (id != model.ResultID) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PutAsync<MessageDto>($"api/results/{id}", ToRequest(model));
        if (!response.Success) { ModelState.AddModelError("", response.Error ?? "Unable to update result."); return View(model); }
        TempData["Success"] = "Result updated.";
        return RedirectToAction(nameof(Event), new { eventId = model.EventID });
    }
    private static object ToRequest(ResultFormViewModel m) => new
    {
        m.FinishTime, m.OverallPosition, m.CategoryPosition
    };
}
