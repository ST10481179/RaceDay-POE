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

✅ Screenshot of successful green build:  


---

## Video Presentation
Unlisted YouTube link:  
👉 [Insert your video link here]

The video demonstrates:
- ERD design decisions
- API endpoint plan choices
- SQL script execution in SSMS

---

## Submission Notes
- All planning documents are inside `/docs`
- Minimum of 20 meaningful commits for Part 1
- SQL script runs cleanly on a fresh SQL Server instance
