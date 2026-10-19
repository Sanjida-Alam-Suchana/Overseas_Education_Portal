using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Controllers;

[ApiController, Route("api")]
public sealed class LookupController(PortalDbContext db, IHttpClientFactory http) : ControllerBase
{
    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken ct) => Ok(new { status = await db.Database.CanConnectAsync(ct) ? "ok" : "unavailable", database = "PostgreSQL" });

    [HttpGet("countries")]
    public async Task<IActionResult> Countries(CancellationToken ct) => Ok(await db.Countries.AsNoTracking().OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(ct));

    [HttpGet("universities")]
    public async Task<IActionResult> Universities([FromQuery] string country, CancellationToken ct)
    {
        try
        {
            var client = http.CreateClient("university-api");
            var records = await client.GetFromJsonAsync<List<UniversityApiItem>>($"search?country={Uri.EscapeDataString(country)}", ct) ?? [];
            return Ok(records.OrderBy(x => x.name).Take(200));
        }
        catch (HttpRequestException) { return Ok(Array.Empty<UniversityApiItem>()); }
    }

    private sealed record UniversityApiItem(string name, string[] web_pages);
}
