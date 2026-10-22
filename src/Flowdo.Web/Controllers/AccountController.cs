using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flowdo.Web.Controllers;

public sealed class AccountController : Controller
{
    [HttpGet("/login")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/tasks");
        }

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = "/tasks"
            },
            GoogleDefaults.AuthenticationScheme
        );
    }

    [Authorize]
    [HttpGet("/account/me")]
    public IActionResult Me()
    {
        var appUserId =
            User.FindFirst("app_user_id")?.Value;

        var displayName =
            User.Identity?.Name ?? "User";

        return Content(
    $"Welcome, {displayName}!\n" +
    $"Your Flowdo ID: {appUserId}\n" +
    "Google authentication successful!",
    "text/plain; charset=utf-8"
);
    }

    [Authorize]
    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return Redirect("/");
    }
}