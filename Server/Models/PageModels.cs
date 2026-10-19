namespace Server.Models;

public sealed record HomePageModel(IReadOnlyList<MentorCardModel> Mentors, IReadOnlyList<CountryCardModel> Countries, IReadOnlyList<GroupSessionCardModel> Sessions);
public sealed record MentorCardModel(int Id, string Name, string University, string Country, string Program, string Service, decimal Rating, int Sessions, string Bio, IReadOnlyList<MentorPlanRequest> Plans);
public sealed record CountryCardModel(string Name, string Title, string Description, int MentorCount);
public sealed record CountryPageModel(CountryCardModel Country, IReadOnlyList<MentorCardModel> Mentors);
public sealed record GroupSessionCardModel(int Id, string Title, string Category, DateTime StartsAt, string Description, string Host, int Attendees, int Capacity);
public sealed record MentorDirectoryPageModel(string? Country, string? Intro, IReadOnlyList<MentorCardModel> Mentors, IReadOnlyList<string> Countries, IReadOnlyList<string> Programs, IReadOnlyList<string> Services, IReadOnlyList<string> Scholarships);
