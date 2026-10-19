using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Repositories;

namespace Server.Controllers;

[ApiController, Route("api/mentors/{id:int}/files/{kind}")]
public sealed class MentorFilesController(PortalDbContext db, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost, RequestSizeLimit(12_000_000), RequireSession("Mentor")]
    public async Task<IActionResult> Upload(int id, string kind, IFormFile file, CancellationToken ct)
    {
        if (id != this.User().AccountId) return Forbid();
        if (kind is not ("photo" or "cv")) return BadRequest(new { message = "Choose photo or cv." });
        if (file.Length == 0 || file.Length > 10_000_000) return BadRequest(new { message = "File must be smaller than 10 MB." });
        var isImage = file.ContentType is "image/jpeg" or "image/png" or "image/webp";
        if ((kind == "photo" && !isImage) || (kind == "cv" && file.ContentType != "application/pdf")) return BadRequest(new { message = kind == "photo" ? "Upload a JPG, PNG, or WebP image." : "Upload a PDF CV." });
        var directory = Path.Combine(env.ContentRootPath, "uploads", id.ToString()); Directory.CreateDirectory(directory);
        var extension = kind == "cv" ? ".pdf" : file.ContentType switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
        var path = Path.Combine(directory, kind + extension);
        await using (var output = System.IO.File.Create(path)) await file.CopyToAsync(output, ct);
        var mentor = await db.MentorProfiles.FirstOrDefaultAsync(x => x.AccountId == id, ct);
        if (mentor is null) return NotFound(new { message = "Save your mentor profile before uploading files." });
        if (kind == "photo") mentor.PhotoPath = path; else mentor.CvPath = path;
        await db.SaveChangesAsync(ct);
        return Ok(new { url = $"/api/mentors/{id}/files/{kind}" });
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, string kind, CancellationToken ct)
    {
        if (kind is not ("photo" or "cv")) return BadRequest();
        var mentor = await db.MentorProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == id, ct);
        var path = kind == "photo" ? mentor?.PhotoPath : mentor?.CvPath;
        if (path is null || !System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, kind == "cv" ? "application/pdf" : "application/octet-stream", Path.GetFileName(path));
    }
}
