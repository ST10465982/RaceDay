using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;
[RequireRole("Organiser")]
public class DashboardController : Controller
{
    private readonly RaceDayApiService _api;
    public DashboardController(RaceDayApiService api) => _api = api;
    public async Task<IActionResult> Index()
    {
        var events = await _api.GetAsync<List<EventDto>>("api/events");
        if (!events.Success) { TempData["Error"] = events.Error; return View(new OrganiserDashboardViewModel()); }
        var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
        var vm = new OrganiserDashboardViewModel
        {
            Events = (events.Data ?? new()).Where(e => e.OrganiserID == userId).OrderBy(e => e.EventDate).ToList()
        };
        foreach (var race in vm.Events)
        {
            var list = await _api.GetAsync<List<EnrollmentDto>>($"api/events/{race.EventID}/enrollments");
            vm.EnrolmentCounts[race.EventID] = (list.Data ?? new()).Count(e => e.EnrollmentStatus != "Cancelled");
        }
        return View(vm);
    }
}
