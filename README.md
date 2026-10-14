# RaceDay-POE
Portfolio of Evidence project for RaceDay system

![Part 2 API CI](https://github.com/ST10481179/RaceDay-POE/actions/workflows/ci.yml/badge.svg?branch=part2-api)

## Project Overview
RaceDay is a web-based event management system designed for South Africa’s road running, walking, and cycling culture.  
The platform enables organisers to create and manage events, while participants can enrol, track results, and view their performance history.

This repository contains the Part 1 planning/database work and the Part 2 ASP.NET Core Web API implementation.

---

## System Roles
- **Organiser**
  - Create, edit, and delete events
  - Manage event categories
  - Capture participant results
  - View all event enrolments

- **Participant**
  - Register and log in
  - Browse and enrol in events
  - View personal enrolments
  - Track personal results

---

## Repository Structure
### /docs
- ERD.png
- POE ERD.pdf
- endpoint-plan.md
- database.sql  
### /src
- `api/RaceDay.WebAPI` - ASP.NET Core API, EF Core models, migrations, Identity, and controllers
- `api/RaceDay.WebAPI.Tests` - xUnit integration tests
### /.github/workflows  
- validate.yml  
**README.md**
---

## CI/CD
A GitHub Actions workflow (`ci.yml`) restores, builds, and tests the .NET 10 API on the `part2-api` branch and pull requests. The existing `validate.yml` continues to validate the Part 1 documentation.

Screenshot of a successful green Part 1 documentation workflow run:
![CI/CD Workflow Screenshot](docs/Screenshot%202026-09-03%20153637.png)

## Part 2 API

The API uses ASP.NET Identity for hashed passwords and roles, JWT bearer authentication, SQL Server, and EF Core Code-First migrations. Organiser and Participant permissions are enforced by authorization policies. Swagger is available at `/swagger` in Development.

The database connection is configured in `src/api/RaceDay.WebAPI/appsettings.json`. Set the JWT signing key using User Secrets for local development:

```powershell
dotnet user-secrets set "Jwt:Key" "<random secret of at least 32 bytes>" --project src/api/RaceDay.WebAPI/RaceDay.WebAPI.csproj
dotnet-ef database update --project src/api/RaceDay.WebAPI/RaceDay.WebAPI.csproj
dotnet run --project src/api/RaceDay.WebAPI/RaceDay.WebAPI.csproj
```

Apply EF migrations to a blank `RaceDayDb`; do not run `docs/database.sql` first, because that Part 1 script creates the same domain tables. The script remains the reference schema/seed artifact, and the API migration creates the mapped ERD tables plus Identity tables.

Run the API integration tests locally with:

```powershell
dotnet test src/api/RaceDay.WebAPI.Tests/RaceDay.WebAPI.Tests.csproj --configuration Release
```

---

## Video Presentation
Unlisted YouTube link:  
https://youtu.be/nIBtFjX2ENQ 

The video demonstrates:
- ERD design decisions
- API endpoint plan choices
- SQL script execution in SSMS

---

## Submission Notes
- All planning documents are inside `/docs`
- Minimum of 20 meaningful commits for Part 1
- SQL script runs cleanly on a fresh SQL Server instance
