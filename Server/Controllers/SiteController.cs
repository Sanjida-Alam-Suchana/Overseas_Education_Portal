using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Repositories;

namespace Server.Controllers;

public sealed class HomeController(PortalDbContext db, IMentorRepository mentors, IGroupSessionRepository sessions) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var mentorRows = await mentors.SearchAsync(null, null, null, null, null, "rating", ct);
        var countryRows = await db.Countries.AsNoTracking().OrderBy(c => c.Name).Select(c => new CountryCardModel(c.Name, c.PageTitle, c.Description, c.Universities.SelectMany(u => u.Mentors).Count())).ToListAsync(ct);
        var sessionRows = await sessions.ListAsync(null, ct);
        var model = new HomePageModel(mentorRows.Take(3).Select(ToCard).ToList(), countryRows, sessionRows.Take(3).Select(ToSessionCard).ToList());
        return View(model);
    }

    [HttpGet("/find-mentor")]
    public async Task<IActionResult> FindMentor([FromQuery] string? country, [FromQuery] string? program, [FromQuery] string? service, [FromQuery] string? scholarship, [FromQuery] string? search, [FromQuery] string? sort, CancellationToken ct)
    {
        var found = await mentors.SearchAsync(country, program, service, scholarship, search, sort, ct);
        var model = new MentorDirectoryPageModel(country, country is null ? "Find a mentor for your next chapter." : $"Explore mentors currently studying in {country}.", found.Select(ToCard).ToList(), await db.Countries.Select(x => x.Name).OrderBy(x => x).ToListAsync(ct), await db.Programs.Select(x => x.Name).OrderBy(x => x).ToListAsync(ct), await db.MentorProfiles.Select(x => x.Service).Distinct().OrderBy(x => x).ToListAsync(ct), await db.Scholarships.Select(x => x.Name).OrderBy(x => x).ToListAsync(ct));
        return View("Mentors", model);
    }

    [HttpGet("/countries/{name}")]
    public async Task<IActionResult> Country(string name, CancellationToken ct)
    {
        var country = await db.Countries.AsNoTracking().FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), ct);
        if (country is null) return NotFound();
        var countryMentors = await mentors.SearchAsync(country.Name, null, null, null, null, "rating", ct);
        var summary = new CountryCardModel(country.Name, country.PageTitle, country.Description, countryMentors.Count);
        return View("Country", new CountryPageModel(summary, countryMentors.Select(ToCard).ToList()));
    }

    [HttpGet("/scholarships")]
    public async Task<IActionResult> Scholarships(CancellationToken ct) => View(await db.Scholarships.AsNoTracking().Include(x => x.Country).OrderBy(x => x.Name).ToListAsync(ct));

    [HttpGet("/group-sessions")]
    public async Task<IActionResult> Sessions([FromQuery] string? category, [FromQuery] string? phase, CancellationToken ct) { var rows = await sessions.ListAsync(category, ct); var past = phase == "past"; return View(rows.Where(x => past ? x.StartsAt <= DateTime.UtcNow : x.StartsAt > DateTime.UtcNow).Select(ToSessionCard).ToList()); }

    [HttpGet("/mentor/{id:int}")]
    public async Task<IActionResult> Mentor(int id, CancellationToken ct) { var mentor = await mentors.GetAsync(id, ct); if (mentor is null) return NotFound(); return View("Mentor", ToCard(mentor)); }

    [HttpGet("/faq")]
    public IActionResult Faq() => View();

    [HttpGet("/my-bookings")]
    public IActionResult MyBookings() => View();

    [HttpGet("/mentor/profile")]
    public IActionResult MentorProfile() => View();

    private static MentorCardModel ToCard(MentorProfile x) => new(x.AccountId, x.Account.Name, x.University.Name, x.University.Country.Name, x.Program.Name, x.Service, x.Rating, x.SessionCount, x.Bio, x.Plans.Select(p => new MentorPlanRequest(p.DurationMinutes, p.Price)).ToList());
    private static GroupSessionCardModel ToSessionCard(GroupSession x) => new(x.Id, x.Title, x.Category, x.StartsAt, x.Description, x.Mentor.Name, x.Registrations.Count, x.Capacity);
}
