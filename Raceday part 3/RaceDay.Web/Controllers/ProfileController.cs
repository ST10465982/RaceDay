using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;
[RequireRole("Organiser", "Participant")]
public class ProfileController : Controller
{
    private readonly RaceDayApiService _api;
    public ProfileController(RaceDayApiService api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var data = await _api.GetAsync<ProfileDto>("api/profile");
        var photo = await _api.GetAsync<ImageUrlDto>("api/media/profile");
        // The existing API returns 404 when a profile has not been created yet.
        var p = data.Data;
        return View(new ProfileFormViewModel
        {
            PhoneNumber = p?.PhoneNumber, DateOfBirth = p?.DateOfBirth,
            EmergencyContactName = p?.EmergencyContactName,
            EmergencyContactNumber = p?.EmergencyContactNumber,
            ExistingPhotoUrl = photo.Data?.Url
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.PutAsync<MessageDto>("api/profile", new
        {
            model.PhoneNumber, model.DateOfBirth,
            model.EmergencyContactName, model.EmergencyContactNumber
        });
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Unable to save profile.");
            return View(model);
        }
        if (model.Photo is not null)
        {
            var upload = await _api.UploadAsync<ImageUrlDto>("api/media/profile", model.Photo);
            if (!upload.Success) TempData["Error"] = "Profile saved, but photo upload failed: " + upload.Error;
        }
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
