using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories;

namespace Server.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAccountRepository accounts, ISessionService sessions, IPasswordHasher<Account> hasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        if (request.Role is not ("Student" or "Mentor")) return BadRequest(new { message = "Choose Student or Mentor." });
        if (await accounts.GetByEmailAsync(request.Email, ct) is not null) return Conflict(new { message = "An account with this email already exists." });
        var account = new Account { Name = request.Name.Trim(), Email = request.Email.Trim().ToLowerInvariant(), Role = request.Role };
        account.PasswordHash = hasher.HashPassword(account, request.Password);
        try { await accounts.CreateAsync(account, ct); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { message = "An account with this email already exists." }); }
        return Ok(await CreateResponse(account, ct));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var account = await accounts.GetByEmailAsync(request.Email, ct);
        if (account is null || hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized(new { message = "Email or password is incorrect." });
        return Ok(await CreateResponse(account, ct));
    }

    [HttpPost("logout"), RequireSession]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Headers.Authorization.ToString()[7..].Trim();
        await sessions.RevokeAsync(token, ct);
        return Ok(new { message = "Signed out." });
    }

    private async Task<object> CreateResponse(Account account, CancellationToken ct)
    {
        var (token, user) = await sessions.CreateAsync(account, ct);
        return new { user.Name, user.Email, user.Role, id = user.AccountId, token };
    }
}
