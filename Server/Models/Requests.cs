using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public sealed record RegisterRequest([Required, StringLength(120)] string Name, [Required, EmailAddress] string Email, [Required, MinLength(8)] string Password, [Required] string Role);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record MentorPlanRequest([Range(20, 40)] int Minutes, [Range(0, 100000)] decimal Price);
public sealed record MentorUpsertRequest([Required] string Name, [Required, EmailAddress] string Email, [Required] string Country, [Required] string University, [Required] string Program, string? Scholarship, [Required] string Service, [Required] string Bio, decimal Rating = 0, int Sessions = 0, List<MentorPlanRequest>? Plans = null);
public sealed record BookingRequest(int MentorId, int Minutes, DateOnly Date, TimeOnly Time);
public sealed record SessionRegistrationRequest;
public sealed record MentorListItem(int Id, string Name, string University, string Country, string Program, string Service, string? Scholarship, decimal Rating, int Sessions, string Bio, string Email, IReadOnlyList<MentorPlanRequest> Plans);
