using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Server.Repositories;

namespace Server.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireSessionAttribute(string? role = null) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var header = context.HttpContext.Request.Headers.Authorization.ToString();
        var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : null;
        var sessions = context.HttpContext.RequestServices.GetRequiredService<ISessionService>();
        var user = await sessions.ValidateAsync(token, context.HttpContext.RequestAborted);
        if (user is null) { context.Result = new UnauthorizedObjectResult(new { message = "Please sign in to continue." }); return; }
        if (role is not null && !string.Equals(user.Role, role, StringComparison.OrdinalIgnoreCase)) { context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden); return; }
        context.HttpContext.Items["CurrentUser"] = user;
    }
}

public static class CurrentUserExtensions
{
    public static SessionUser User(this ControllerBase controller) => (SessionUser)controller.HttpContext.Items["CurrentUser"]!;
}
