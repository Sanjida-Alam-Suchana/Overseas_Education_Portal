using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories;

public interface IMentorRepository
{
    Task<IReadOnlyList<MentorProfile>> SearchAsync(string? country, string? program, string? service, string? scholarship, string? search, string? sort, CancellationToken ct);
    Task<MentorProfile?> GetAsync(int id, CancellationToken ct);
    Task<MentorProfile> SaveAsync(int accountId, MentorUpsertRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int accountId, CancellationToken ct);
}

public interface IAccountRepository
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct);
    Task<Account?> GetByIdAsync(int id, CancellationToken ct);
    Task<Account> CreateAsync(Account account, CancellationToken ct);
}

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> ForStudentAsync(int studentId, CancellationToken ct);
    Task<Booking> CreateAsync(int studentId, BookingRequest request, CancellationToken ct);
}

public interface IGroupSessionRepository
{
    Task<IReadOnlyList<GroupSession>> ListAsync(string? category, CancellationToken ct);
    Task<SessionRegistration> RegisterAsync(int sessionId, int studentId, CancellationToken ct);
}

public sealed class MentorRepository(PortalDbContext db) : IMentorRepository
{
    private IQueryable<MentorProfile> Query() => db.MentorProfiles.AsNoTracking()
        .Include(x => x.Account).Include(x => x.University).ThenInclude(x => x.Country)
        .Include(x => x.Program).Include(x => x.Scholarship).Include(x => x.Plans);

    public async Task<IReadOnlyList<MentorProfile>> SearchAsync(string? country, string? program, string? service, string? scholarship, string? search, string? sort, CancellationToken ct)
    {
        var q = Query();
        if (!string.IsNullOrWhiteSpace(country)) q = q.Where(x => x.University.Country.Name == country);
        if (!string.IsNullOrWhiteSpace(program)) q = q.Where(x => x.Program.Name.Contains(program));
        if (!string.IsNullOrWhiteSpace(service)) q = q.Where(x => x.Service.Contains(service));
        if (!string.IsNullOrWhiteSpace(scholarship)) q = q.Where(x => x.Scholarship != null && x.Scholarship.Name.Contains(scholarship));
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Account.Name.Contains(search) || x.University.Name.Contains(search));
        q = sort switch { "sessions" => q.OrderByDescending(x => x.SessionCount), "price" => q.OrderBy(x => x.Plans.Min(p => p.Price)), _ => q.OrderByDescending(x => x.Rating) };
        return await q.ToListAsync(ct);
    }

    public Task<MentorProfile?> GetAsync(int id, CancellationToken ct) => Query().FirstOrDefaultAsync(x => x.AccountId == id, ct);

    public async Task<MentorProfile> SaveAsync(int accountId, MentorUpsertRequest request, CancellationToken ct)
    {
        var country = await db.Countries.FirstOrDefaultAsync(x => x.Name == request.Country, ct);
        if (country is null) { country = new Country { Name = request.Country, PageTitle = $"Study in {request.Country}", Description = $"Meet current students and mentors who can help you plan to study in {request.Country}." }; db.Countries.Add(country); }
        var university = await db.Universities.FirstOrDefaultAsync(x => x.Name == request.University && x.Country.Name == request.Country, ct);
        if (university is null) { university = new University { Name = request.University, Country = country }; db.Universities.Add(university); }
        var program = await db.Programs.FirstOrDefaultAsync(x => x.Name == request.Program, ct);
        if (program is null) { program = new ProgramOfStudy { Name = request.Program }; db.Programs.Add(program); }
        Scholarship? scholarship = null;
        if (!string.IsNullOrWhiteSpace(request.Scholarship)) { scholarship = await db.Scholarships.FirstOrDefaultAsync(x => x.Name == request.Scholarship, ct); if (scholarship is null) { scholarship = new Scholarship { Name = request.Scholarship, Country = country }; db.Scholarships.Add(scholarship); } }
        var mentor = await db.MentorProfiles.Include(x => x.Plans).FirstOrDefaultAsync(x => x.AccountId == accountId, ct);
        if (mentor is null) { mentor = new MentorProfile { AccountId = accountId }; db.MentorProfiles.Add(mentor); }
        mentor.University = university; mentor.Program = program; mentor.Scholarship = scholarship; mentor.Service = request.Service; mentor.Bio = request.Bio; mentor.Rating = request.Rating; mentor.SessionCount = request.Sessions;
        foreach (var planRequest in request.Plans ?? [])
        {
            if (planRequest.Minutes is not (20 or 40)) throw new ArgumentException("Mentor plans must be 20 or 40 minutes.");
            var plan = mentor.Plans.FirstOrDefault(x => x.DurationMinutes == planRequest.Minutes);
            if (plan is null) mentor.Plans.Add(new MentorPlan { DurationMinutes = planRequest.Minutes, Price = planRequest.Price }); else plan.Price = planRequest.Price;
        }
        await db.SaveChangesAsync(ct);
        return (await GetAsync(accountId, ct))!;
    }

    public async Task<bool> DeleteAsync(int accountId, CancellationToken ct)
    {
        var mentor = await db.MentorProfiles.Include(x => x.Account).FirstOrDefaultAsync(x => x.AccountId == accountId, ct);
        if (mentor is null) return false;
        db.MentorProfiles.Remove(mentor);
        await db.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class AccountRepository(PortalDbContext db) : IAccountRepository
{
    public Task<Account?> GetByEmailAsync(string email, CancellationToken ct) => db.Accounts.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), ct);
    public Task<Account?> GetByIdAsync(int id, CancellationToken ct) => db.Accounts.FindAsync([id], ct).AsTask();
    public async Task<Account> CreateAsync(Account account, CancellationToken ct) { db.Accounts.Add(account); await db.SaveChangesAsync(ct); return account; }
}

public sealed class BookingRepository(PortalDbContext db) : IBookingRepository
{
    public async Task<IReadOnlyList<Booking>> ForStudentAsync(int studentId, CancellationToken ct) => await db.Bookings.AsNoTracking().Include(x => x.Mentor).ThenInclude(x => x.Account).Include(x => x.MentorPlan).Where(x => x.StudentId == studentId).OrderByDescending(x => x.StartsAt).ToListAsync(ct);
    public async Task<Booking> CreateAsync(int studentId, BookingRequest request, CancellationToken ct)
    {
        if (request.Minutes is not (20 or 40)) throw new ArgumentException("Choose a 20 or 40 minute call.");
        var plan = await db.MentorPlans.FirstOrDefaultAsync(x => x.MentorId == request.MentorId && x.DurationMinutes == request.Minutes, ct) ?? throw new KeyNotFoundException("That mentor plan was not found.");
        var startsAt = DateTime.SpecifyKind(request.Date.ToDateTime(request.Time), DateTimeKind.Utc);
        var booking = new Booking { StudentId = studentId, MentorId = request.MentorId, MentorPlanId = plan.Id, StartsAt = startsAt, Status = "Confirmed" };
        db.Bookings.Add(booking); await db.SaveChangesAsync(ct); return booking;
    }
}

public sealed class GroupSessionRepository(PortalDbContext db) : IGroupSessionRepository
{
    public async Task<IReadOnlyList<GroupSession>> ListAsync(string? category, CancellationToken ct)
    {
        var q = db.GroupSessions.AsNoTracking().Include(x => x.Mentor).Include(x => x.Registrations).AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category);
        return await q.OrderBy(x => x.StartsAt).ToListAsync(ct);
    }
    public async Task<SessionRegistration> RegisterAsync(int sessionId, int studentId, CancellationToken ct)
    {
        var session = await db.GroupSessions.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == sessionId, ct) ?? throw new KeyNotFoundException("Session not found.");
        if (session.StartsAt <= DateTime.UtcNow) throw new InvalidOperationException("Registration closed because the session has passed.");
        if (session.Registrations.Count >= session.Capacity) throw new InvalidOperationException("This session is full.");
        var registration = new SessionRegistration { SessionId = sessionId, StudentId = studentId };
        db.SessionRegistrations.Add(registration); await db.SaveChangesAsync(ct); return registration;
    }
}
