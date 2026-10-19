using System.Text;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;

namespace Server.Controllers;

[ApiController, Route("api/exports")]
public sealed class ExportsController(IMentorRepository mentors) : ControllerBase
{
    [HttpGet("mentors.csv")]
    public async Task<IActionResult> Csv(CancellationToken ct)
    {
        var rows = await mentors.SearchAsync(null, null, null, null, null, "rating", ct);
        var csv = new StringBuilder("Name,University,Country,Program,Service,Rating,Sessions\n");
        foreach (var x in rows) csv.AppendLine(string.Join(',', new[] { x.Account.Name, x.University.Name, x.University.Country.Name, x.Program.Name, x.Service, x.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture), x.SessionCount.ToString() }.Select(v => $"\"{v.Replace("\"", "\"\"")}\"")));
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", "mentors.csv");
    }

    [HttpGet("mentors.pdf")]
    public async Task<IActionResult> Printable(CancellationToken ct)
    {
        var rows = await mentors.SearchAsync(null, null, null, null, null, "rating", ct);
        var body = string.Join("", rows.Select(x => $"<tr><td>{E(x.Account.Name)}</td><td>{E(x.University.Name)}</td><td>{E(x.University.Country.Name)}</td><td>{E(x.Service)}</td></tr>"));
        return Content($"<!doctype html><html><meta charset='utf-8'><title>Mentor directory</title><body><h1>Mentor directory</h1><button onclick='print()'>Print / Save as PDF</button><table border='1' cellpadding='8'><tr><th>Name</th><th>University</th><th>Country</th><th>Service</th></tr>{body}</table></body></html>", "text/html");
    }
    private static string E(string text) => System.Net.WebUtility.HtmlEncode(text);
}
