using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public sealed class PortalDbContext(DbContextOptions<PortalDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<University> Universities => Set<University>();
    public DbSet<ProgramOfStudy> Programs => Set<ProgramOfStudy>();
    public DbSet<Scholarship> Scholarships => Set<Scholarship>();
    public DbSet<MentorProfile> MentorProfiles => Set<MentorProfile>();
    public DbSet<MentorPlan> MentorPlans => Set<MentorPlan>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<GroupSession> GroupSessions => Set<GroupSession>();
    public DbSet<SessionRegistration> SessionRegistrations => Set<SessionRegistration>();
    public DbSet<LoginSession> LoginSessions => Set<LoginSession>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Account>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Account>().HasCheckConstraint("CK_Accounts_Role", "\"Role\" IN ('Student','Mentor')");
        b.Entity<Country>().HasIndex(x => x.Name).IsUnique();
        b.Entity<University>().HasIndex(x => new { x.CountryId, x.Name }).IsUnique();
        b.Entity<ProgramOfStudy>().HasIndex(x => x.Name).IsUnique();
        b.Entity<Scholarship>().HasIndex(x => x.Name).IsUnique();
        b.Entity<MentorProfile>().HasOne(x => x.Account).WithOne(x => x.MentorProfile).HasForeignKey<MentorProfile>(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<MentorProfile>().HasAlternateKey(x => x.AccountId);
        b.Entity<MentorProfile>().HasOne(x => x.University).WithMany(x => x.Mentors).HasForeignKey(x => x.UniversityId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<MentorProfile>().HasOne(x => x.Program).WithMany(x => x.Mentors).HasForeignKey(x => x.ProgramOfStudyId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<MentorProfile>().HasOne(x => x.Scholarship).WithMany(x => x.Mentors).HasForeignKey(x => x.ScholarshipId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<MentorPlan>().HasIndex(x => new { x.MentorId, x.DurationMinutes }).IsUnique();
        b.Entity<MentorPlan>().HasOne(x => x.Mentor).WithMany(x => x.Plans).HasForeignKey(x => x.MentorId).HasPrincipalKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<MentorPlan>().HasCheckConstraint("CK_MentorPlans_Duration", "\"DurationMinutes\" IN (20,40)");
        b.Entity<MentorPlan>().Property(x => x.Price).HasPrecision(10, 2);
        b.Entity<Booking>().HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.Mentor).WithMany().HasForeignKey(x => x.MentorId).HasPrincipalKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Booking>().HasOne(x => x.MentorPlan).WithMany().HasForeignKey(x => x.MentorPlanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<GroupSession>().HasOne(x => x.Mentor).WithMany().HasForeignKey(x => x.MentorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SessionRegistration>().HasIndex(x => new { x.SessionId, x.StudentId }).IsUnique();
        b.Entity<SessionRegistration>().HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<LoginSession>().HasIndex(x => x.TokenHash).IsUnique();
    }
}
