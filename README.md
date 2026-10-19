# AbroadMates Clone

Responsive study abroad mentorship portal using ASP.NET Core MVC, Razor views, PostgreSQL/EF Core, and HTML/CSS/JavaScript.

## Start with your pgAdmin database

You have already created the PostgreSQL server and a database named `overseaseducation` in pgAdmin. The app creates the relational tables and demo rows on its first successful start.

1. In pgAdmin, right-click the `overseaseducation` database and confirm it is connected and empty (or that it has no tables from an earlier attempt).
2. Open a terminal in `Overseas-Education-Portal/Server`.
3. Open `appsettings.json` and replace `YOUR_POSTGRES_PASSWORD` in `ConnectionStrings:Portal` with your PostgreSQL password. Then, from the `Server` folder, apply the initial migration and start the app:

   ```sh
   dotnet restore
   dotnet ef database update
   dotnet run
   ```

5. Visit `http://localhost:5000`. In pgAdmin, right-click `overseaseducation` → **Refresh** → **Schemas → public → Tables** to see the tables and seeded sample mentors/sessions.

The initial migration is `Data/Migrations/20261019053214_InitialCreate.cs`; it creates all 11 tables, indexes, constraints, and relationships. The app also applies pending migrations on startup and inserts demo rows. For future model changes, create a migration with `dotnet ef migrations add MigrationName`, then apply it with `dotnet ef database update`.

## MVC structure

- `Models/`: relational entities, request DTOs, and page view models.
- `Data/PortalDbContext.cs`: PostgreSQL EF Core relationships, constraints, and indexes. This is the database schema source.
- `Data/DatabaseInitializer.cs`: applies pending migrations and inserts sample data.
- `Repositories/`: mentor, account, booking, group-session, and login-session data access.
- `Controllers/`: API controllers and MVC page controllers.
- `Views/`: Razor pages for home, mentor search and profile, country, scholarships, sessions, bookings, FAQ, and account login/registration.
- `wwwroot/`: responsive CSS and JavaScript.

## Pages

`/`, `/find-mentor`, `/countries/{name}`, `/mentor/{id}`, `/scholarships`, `/group-sessions`, `/my-bookings`, `/mentor/profile`, `/faq`, `/login`, `/register`.

## API

- `GET /api/mentors` supports country, program, service, scholarship, search, and sort filters; mentor POST/PUT/DELETE endpoints are included.
- `/api/auth/register`, `/api/auth/login`, `/api/auth/logout` uses ASP.NET password hashing and expiring bearer sessions stored in PostgreSQL.
- `GET/POST /api/bookings` handles student bookings; `GET /api/sessions` and `POST /api/sessions/{id}/register` handle group sessions.
- `/api/universities?country=Canada` loads universities from the public Hipolabs Universities API.
- `/api/mentors/{id}/files/{photo|cv}` handles profile uploads and downloads.
- `/api/exports/mentors.csv` and `/api/exports/mentors.pdf` provide Excel-compatible CSV and a printable PDF view.

