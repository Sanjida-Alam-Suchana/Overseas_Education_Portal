using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Server.Controllers;
using Server.Data;
using Server.Models;
using Server.Repositories;

var builder = WebApplication.CreateBuilder(args);
var connectionStringBase = builder.Configuration.GetConnectionString("Portal")
    ?? throw new InvalidOperationException("Set ConnectionStrings:Portal in appsettings.json or the ConnectionStrings__Portal environment variable.");
var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionStringBase);
var password = builder.Configuration["Database:Password"];
if (!string.IsNullOrWhiteSpace(password)) connectionBuilder.Password = password;
if (string.IsNullOrWhiteSpace(connectionBuilder.Password))
    throw new InvalidOperationException("PostgreSQL password is missing. Set Database:Password or set a password in ConnectionStrings:Portal using .NET User Secrets.");
var connectionString = connectionBuilder.ConnectionString;

builder.Services.AddDbContext<PortalDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("university-api", client => client.BaseAddress = new Uri("https://universities.hipolabs.com/"));
builder.Services.AddScoped<IMentorRepository, MentorRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IGroupSessionRepository, GroupSessionRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<RequireSessionAttribute>();
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
    await DatabaseInitializer.InitializeAsync(db);
}

app.Run();
