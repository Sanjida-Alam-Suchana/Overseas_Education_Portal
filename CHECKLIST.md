# AbroadMates Clone Checklist

Based on `word` and a review of the public AbroadMates homepage.

## Implemented
- [x] Responsive public home, mentor directory, country pages, scholarship list, group sessions, FAQ, and footer in MVC/Razor.
- [x] Models for accounts, countries, universities, programs, scholarships, mentor profiles and plans, bookings, group sessions, registrations, and login sessions.
- [x] PostgreSQL relational schema using EF Core/Npgsql with unique constraints and foreign keys.
- [x] Repository layer for mentor, account, booking, and group-session access.
- [x] MVC page controllers plus JSON API controllers.
- [x] Mentor filtering/sorting, profile and plan CRUD, 20/40 minute call booking, session registration with past-date/capacity checks, and bookings page.
- [x] Account registration/login with password hashing and expiring bearer sessions persisted in PostgreSQL.
- [x] University list API integration, profile photo/CV upload/download, mentor CSV export, and printable PDF view.
- [x] EF Core initializes and seeds a fresh database when the app starts.
- [x] Removed the duplicate standalone client and unused scaffold/reference files; MVC `wwwroot` is the single frontend source.

## To do on your computer
- [ ] Add the PostgreSQL password to local .NET User Secrets and start the app; then confirm the generated tables in pgAdmin.
- [ ] Create an AbroadMates account to inspect authenticated website flows (account credentials were not provided).

## Database note

The app uses PostgreSQL now. Do not run any SQL Server version of the schema. `Server/appsettings.json` names the local database `overseaseducation`; supply its password through `Database__Password` when starting the app.
