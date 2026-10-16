# RaceDay-POE (Part 1)
Portfolio of Evidence project for RaceDay system
## Project Overview
RaceDay is a web-based event management system designed for South Africa’s road running, walking, and cycling culture.  
The platform enables organisers to create and manage events, while participants can enrol, track results, and view their performance history.

This repository contains **Part 1: Planning and Database** of the Portfolio of Evidence (PoE).  
It includes the ERD, API endpoint plan, and SQL database script.

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
- (empty for Part 1, will be used in Part 2 & 3)  
### /.github/workflows  
- validate.yml  
**README.md**
---

## CI/CD
A GitHub Actions workflow (`validate.yml`) is configured to:
- Check that the `/docs` folder exists
- Verify that `ERD.png`, `endpoint-plan.md`, and `database.sql` are present

Screenshot of successful green build:  
![CI/CD Workflow Screenshot](docs/Screenshot%202026-09-03%20153637.png)

---

## Video Presentation
Unlisted YouTube link:  
https://youtu.be/nIBtFjX2ENQ 

The video demonstrates:
- ERD design decisions
- API endpoint plan choices
- SQL script execution in SSMS

---
## Part 2 – Web API Implementation
## Video Presentation Part 2
Unlisted YouTube link:
https://youtu.be/MBRmk7sk56k
### Project Setup
- ASP.NET Core WebAPI project located at `src/api/RaceDay/RaceDay.csproj`.
- Entity Framework Core with migrations configured.
- Connection string defined in `appsettings.json`.

### Authentication & Roles
- Identity configured with hashed passwords.
- Roles: **Organiser** and **Participant**.
- JWT authentication implemented with role claims.

### Functional Endpoints
- **AuthenticationController** → Register, Login.
- **UserController** → Manage user profiles.
- **EventController** → Organisers create/update/delete events; all users view.
- **CategoryController** → Organisers define categories; all users view.
- **EnrolmentController** → Participants enrol; Organisers view enrolments.
- **ResultController** → Organisers capture results; Participants view results.

### Swagger Integration
- Swagger enabled in `Program.cs`.
- Accessible at `/swagger` when running the API.
- Endpoints documented with summaries and response types.

### Unit Testing
- xUnit tests included.
- Covers both success and failure scenarios (e.g., unauthenticated requests, wrong role, duplicate enrolments).

### CI/CD
- GitHub Actions workflow (`.github/workflows/ci.yml`) builds and tests on push to `main`.
- Status badge included below:

![.NET CI](https://github.com/ST10481179/RaceDay-POE/actions/workflows/ci.yml/badge.svg)

---

## How to Run
1. Clone the repo:
   ```bash
   git clone https://github.com/ST10481179/RaceDay-POE.git
2. Navigate to the API project:
   cd src/api/RaceDay
3. Apply migrations:
  dotnet ef database update
4. Run the API:
  dotnet run
5. Open Swagger UI:
   https://localhost:5001/swagger

   

## Submission Notes
- All planning documents are inside `/docs`
- Minimum of 20 meaningful commits for Part 1
- SQL script runs cleanly on a fresh SQL Server instance
