using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

public sealed class AccountController : Controller
{
    [HttpGet("/login")] public IActionResult Login() => View();
    [HttpGet("/register")] public IActionResult Register() => View();
}
