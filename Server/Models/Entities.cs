using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public sealed class Account
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required] public string Role { get; set; } = "Student";
    public MentorProfile? MentorProfile { get; set; }
}

public sealed class Country
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(180)] public string PageTitle { get; set; } = "";
    [Required, StringLength(600)] public string Description { get; set; } = "";
    public ICollection<University> Universities { get; set; } = new List<University>();
    public ICollection<Scholarship> Scholarships { get; set; } = new List<Scholarship>();
}

public sealed class University
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = "";
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;
    public ICollection<MentorProfile> Mentors { get; set; } = new List<MentorProfile>();
}

public sealed class ProgramOfStudy
{
    public int Id { get; set; }
    [Required, StringLength(160)] public string Name { get; set; } = "";
    public ICollection<MentorProfile> Mentors { get; set; } = new List<MentorProfile>();
}

public sealed class Scholarship
{
    public int Id { get; set; }
    [Required, StringLength(160)] public string Name { get; set; } = "";
    public int? CountryId { get; set; }
    public Country? Country { get; set; }
    public ICollection<MentorProfile> Mentors { get; set; } = new List<MentorProfile>();
}

public sealed class MentorProfile
{
    [Key] public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public int UniversityId { get; set; }
    public University University { get; set; } = null!;
    public int ProgramOfStudyId { get; set; }
    public ProgramOfStudy Program { get; set; } = null!;
    public int? ScholarshipId { get; set; }
    public Scholarship? Scholarship { get; set; }
    [Required, StringLength(160)] public string Service { get; set; } = "";
    [Required, StringLength(2000)] public string Bio { get; set; } = "";
    public decimal Rating { get; set; }
    public int SessionCount { get; set; }
    public string? PhotoPath { get; set; }
    public string? CvPath { get; set; }
    public ICollection<MentorPlan> Plans { get; set; } = new List<MentorPlan>();
}

public sealed class MentorPlan
{
    public int Id { get; set; }
    public int MentorId { get; set; }
    public MentorProfile Mentor { get; set; } = null!;
    [Range(20, 40)] public int DurationMinutes { get; set; }
    [Range(0, 100000)] public decimal Price { get; set; }
}

public sealed class Booking
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Account Student { get; set; } = null!;
    public int MentorId { get; set; }
    public MentorProfile Mentor { get; set; } = null!;
    public int MentorPlanId { get; set; }
    public MentorPlan MentorPlan { get; set; } = null!;
    public DateTime StartsAt { get; set; }
    [Required, StringLength(30)] public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class GroupSession
{
    public int Id { get; set; }
    public int MentorId { get; set; }
    public Account Mentor { get; set; } = null!;
    [Required, StringLength(200)] public string Title { get; set; } = "";
    [Required, StringLength(100)] public string Category { get; set; } = "";
    public DateTime StartsAt { get; set; }
    [Required, StringLength(1200)] public string Description { get; set; } = "";
    public int Capacity { get; set; } = 50;
    public ICollection<SessionRegistration> Registrations { get; set; } = new List<SessionRegistration>();
}

public sealed class SessionRegistration
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public GroupSession Session { get; set; } = null!;
    public int StudentId { get; set; }
    public Account Student { get; set; } = null!;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public sealed class LoginSession
{
    public int Id { get; set; }
    [Required] public string TokenHash { get; set; } = "";
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
