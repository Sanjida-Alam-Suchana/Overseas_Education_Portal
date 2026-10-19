using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(PortalDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        if (await db.Countries.AnyAsync(ct)) return;
        var canada = new Country { Name = "Canada", PageTitle = "Study in Canada", Description = "Meet students who can help you plan your studies, applications, and life in Canada." };
        var australia = new Country { Name = "Australia", PageTitle = "Study in Australia", Description = "Get first-hand advice from students who have made Australia their home." };
        var cs = new ProgramOfStudy { Name = "Computer Science" };
        var business = new ProgramOfStudy { Name = "Business" };
        var publicHealth = new ProgramOfStudy { Name = "Public Health" };
        var toronto = new University { Name = "University of Toronto", Country = canada };
        var melbourne = new University { Name = "University of Melbourne", Country = australia };
        var chevening = new Scholarship { Name = "Chevening", Country = new Country { Name = "United Kingdom", PageTitle = "Study in the United Kingdom", Description = "Find study abroad mentors." } };
        var ayesha = new Account { Name = "Ayesha Rahman", Email = "ayesha@example.com", Role = "Mentor", PasswordHash = "seed-account-cannot-login" };
        var daniel = new Account { Name = "Daniel Kim", Email = "daniel@example.com", Role = "Mentor", PasswordHash = "seed-account-cannot-login" };
        var maya = new Account { Name = "Maya Chowdhury", Email = "maya@example.com", Role = "Mentor", PasswordHash = "seed-account-cannot-login" };
        db.MentorProfiles.AddRange(
            new MentorProfile { Account = ayesha, University = toronto, Program = cs, Scholarship = chevening, Service = "Scholarship guidance", Bio = "A current graduate student helping applicants build a clear, realistic study plan.", Rating = 4.9m, SessionCount = 82, Plans = [new() { DurationMinutes = 20, Price = 18 }, new() { DurationMinutes = 40, Price = 32 }] },
            new MentorProfile { Account = daniel, University = melbourne, Program = business, Service = "Application review", Bio = "I help international students make confident decisions about applications and campus life.", Rating = 4.8m, SessionCount = 64, Plans = [new() { DurationMinutes = 20, Price = 20 }, new() { DurationMinutes = 40, Price = 36 }] },
            new MentorProfile { Account = maya, University = toronto, Program = publicHealth, Service = "Visa advice", Bio = "Practical advice from choosing programs through settling into a new country.", Rating = 4.7m, SessionCount = 51, Plans = [new() { DurationMinutes = 20, Price = 15 }, new() { DurationMinutes = 40, Price = 28 }] });
        await db.SaveChangesAsync(ct);
        db.GroupSessions.AddRange(
            new GroupSession { MentorId = ayesha.Id, Title = "Writing a scholarship-winning personal statement", Category = "Scholarships", StartsAt = DateTime.UtcNow.AddDays(5), Description = "A practical workshop for turning your experiences into a focused personal statement." },
            new GroupSession { MentorId = daniel.Id, Title = "Your first month in Australia", Category = "Student life", StartsAt = DateTime.UtcNow.AddDays(9), Description = "Ask current students about housing, classes, and settling in." },
            new GroupSession { MentorId = maya.Id, Title = "Choosing the right Canadian graduate program", Category = "Applications", StartsAt = DateTime.UtcNow.AddDays(-8), Description = "A conversation about building a balanced program shortlist." });
        await db.SaveChangesAsync(ct);
    }
}
