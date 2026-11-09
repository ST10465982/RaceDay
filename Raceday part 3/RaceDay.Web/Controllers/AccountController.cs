using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly RaceDayApiService _api;
    public AccountController(RaceDayApiService api) => _api = api;

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _api.PostAsync<MessageDto>("api/auth/register", new
        {
            model.FirstName, model.LastName, model.Email, model.Password, model.Role
        });
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Registration failed.");
            return View(model);
        }
        TempData["Success"] = "Account created successfully. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => HttpContext.Session.GetInt32("UserID") is not null
        ? RedirectToAction("Index", "Home") : View(new LoginViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        HttpContext.Session.Clear();
        _api.ClearApiSession();
        var result = await _api.PostAsync<UserDto>("api/auth/login", new { model.Email, model.Password });
        if (!result.Success || result.Data is null || result.Data.UserID <= 0 ||
            (result.Data.Role != "Organiser" && result.Data.Role != "Participant"))
        {
            ModelState.AddModelError("", result.Error ?? "Invalid login response.");
            return View(model);
        }
        HttpContext.Session.SetInt32("UserID", result.Data.UserID);
        HttpContext.Session.SetString("Role", result.Data.Role);
        HttpContext.Session.SetString("FirstName", result.Data.FirstName);
        HttpContext.Session.SetString("Email", result.Data.Email);
        TempData["Success"] = $"Welcome back, {result.Data.FirstName}!";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Logging out of the API invalidates the server-side API session.
        await _api.PostAsync<MessageDto>("api/auth/logout", new { });
        _api.ClearApiSession();
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
