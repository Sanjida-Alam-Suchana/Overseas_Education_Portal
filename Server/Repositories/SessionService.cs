using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories;

public sealed record SessionUser(int AccountId, string Name, string Email, string Role);

public interface ISessionService
{
    Task<(string Token, SessionUser User)> CreateAsync(Account account, CancellationToken ct);
    Task<SessionUser?> ValidateAsync(string? token, CancellationToken ct);
    Task RevokeAsync(string? token, CancellationToken ct);
}

public sealed class SessionService(PortalDbContext db) : ISessionService
{
    public async Task<(string Token, SessionUser User)> CreateAsync(Account account, CancellationToken ct)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        db.LoginSessions.Add(new LoginSession { AccountId = account.Id, TokenHash = tokenHash, ExpiresAt = DateTime.UtcNow.AddHours(12) });
        await db.SaveChangesAsync(ct);
        return (token, new(account.Id, account.Name, account.Email, account.Role));
    }

    public async Task<SessionUser?> ValidateAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var row = await db.LoginSessions.AsNoTracking().Include(x => x.Account).FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.ExpiresAt > DateTime.UtcNow, ct);
        return row is null ? null : new(row.AccountId, row.Account.Name, row.Account.Email, row.Account.Role);
    }

    public async Task RevokeAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return;
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var rows = await db.LoginSessions.Where(x => x.TokenHash == tokenHash).ToListAsync(ct);
        db.LoginSessions.RemoveRange(rows); await db.SaveChangesAsync(ct);
    }
}
