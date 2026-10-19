using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace Server.Controllers;

[ApiController, Route("api/sessions")]
public sealed class GroupSessionsController(IGroupSessionRepository sessions) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? category, CancellationToken ct)
    {
        var rows = await sessions.ListAsync(category, ct);
        return Ok(rows.Select(x => new { x.Id, x.Title, x.Category, date = x.StartsAt, x.Description, host = x.Mentor.Name, attendees = x.Registrations.Count, x.Capacity }));
    }

    [HttpPost("{id:int}/register"), RequireSession("Student")]
    public async Task<IActionResult> Register(int id, CancellationToken ct)
    {
        try { var row = await sessions.RegisterAsync(id, this.User().AccountId, ct); return Created($"/api/session-registrations/{row.Id}", new { row.Id, row.SessionId, studentEmail = this.User().Email, row.RegisteredAt }); }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { message = "You are already registered for this session." }); }
    }
}
