using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;
[RequireRole("Organiser")]
public class CategoriesController : Controller
{
    private readonly RaceDayApiService _api;
    public CategoriesController(RaceDayApiService api) => _api = api;
    public async Task<IActionResult> Index(int eventId)
    {
        var response = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");
        if (!response.Success) TempData["Error"] = response.Error;
        ViewBag.EventID = eventId;
        return View(response.Data ?? new());
    }
    [HttpGet]
    public IActionResult Create(int eventId) => View(new CategoryFormViewModel { EventID = eventId });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PostAsync<MessageDto>($"api/events/{model.EventID}/categories", ToRequest(model));
        if (!response.Success) { ModelState.AddModelError("", response.Error ?? "Category not created."); return View(model); }
        TempData["Success"] = "Category added.";
        return RedirectToAction(nameof(Index), new { eventId = model.EventID });
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _api.GetAsync<CategoryDto>($"api/categories/{id}");
        if (!response.Success || response.Data is null) return NotFound();
        var c = response.Data;
        return View(new CategoryFormViewModel
        {
            CategoryID = c.CategoryID, EventID = c.EventID, CategoryName = c.CategoryName,
            CategoryType = c.CategoryType, MinimumAge = c.MinimumAge,
            MaximumAge = c.MaximumAge, DistanceKm = c.DistanceKm, Description = c.Description
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (id != model.CategoryID) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        var response = await _api.PutAsync<MessageDto>($"api/categories/{id}", ToRequest(model));
        if (!response.Success) { ModelState.AddModelError("", response.Error ?? "Update failed."); return View(model); }
        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Index), new { eventId = model.EventID });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        var response = await _api.DeleteAsync<MessageDto>($"api/categories/{id}");
        TempData[response.Success ? "Success" : "Error"] = response.Success ? "Category deleted." : response.Error;
        return RedirectToAction(nameof(Index), new { eventId });
    }
    private static object ToRequest(CategoryFormViewModel c) => new
    {
        c.CategoryName, c.CategoryType, c.MinimumAge, c.MaximumAge, c.DistanceKm, c.Description
    };
}
