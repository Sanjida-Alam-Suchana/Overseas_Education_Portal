using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories;

namespace Server.Controllers;

[ApiController, Route("api/mentors")]
public sealed class MentorsController(IMentorRepository mentors) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MentorListItem>>> List([FromQuery] string? country, [FromQuery] string? program, [FromQuery] string? service, [FromQuery] string? scholarship, [FromQuery] string? search, [FromQuery] string? sort, CancellationToken ct)
    {
        var rows = await mentors.SearchAsync(country, program, service, scholarship, search, sort, ct);
        return Ok(rows.Select(ToItem).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MentorListItem>> Get(int id, CancellationToken ct)
    {
        var mentor = await mentors.GetAsync(id, ct);
        return mentor is null ? NotFound() : Ok(ToItem(mentor));
    }

    [HttpPost, RequireSession("Mentor")]
    public async Task<IActionResult> Create(MentorUpsertRequest request, CancellationToken ct)
    {
        var user = this.User();
        if (request.Email.Trim().ToLowerInvariant() != user.Email) return StatusCode(StatusCodes.Status403Forbidden);
        var mentor = await mentors.SaveAsync(user.AccountId, request, ct);
        return CreatedAtAction(nameof(Get), new { id = mentor.AccountId }, ToItem(mentor));
    }

    [HttpPut("{id:int}"), RequireSession("Mentor")]
    public async Task<IActionResult> Update(int id, MentorUpsertRequest request, CancellationToken ct)
    {
        if (id != this.User().AccountId) return StatusCode(StatusCodes.Status403Forbidden);
        var mentor = await mentors.SaveAsync(id, request, ct);
        return Ok(ToItem(mentor));
    }

    [HttpDelete("{id:int}"), RequireSession("Mentor")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (id != this.User().AccountId) return StatusCode(StatusCodes.Status403Forbidden);
        return await mentors.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }

    private static MentorListItem ToItem(MentorProfile x) => new(x.AccountId, x.Account.Name, x.University.Name, x.University.Country.Name, x.Program.Name, x.Service, x.Scholarship?.Name, x.Rating, x.SessionCount, x.Bio, x.Account.Email, x.Plans.OrderBy(p => p.DurationMinutes).Select(p => new MentorPlanRequest(p.DurationMinutes, p.Price)).ToList());
}
