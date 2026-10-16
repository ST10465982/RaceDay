# RaceDay

RaceDay is a web-based event management system developed for the South African road running, walking and cycling community. The system provides a central platform for Organisers to manage events and for Participants to register for events, select categories and view race results.

The project is developed progressively across the RaceDay Portfolio of Evidence.

---

# System Roles

## Organiser

The Organiser is responsible for managing RaceDay events.

An Organiser can:

- Register and log in.
- Manage their profile.
- Create events.
- Edit events they created.
- Delete eligible events.
- Create and manage event categories.
- View participants enrolled in their events.
- Capture race results.
- Update race results.

## Participant

The Participant uses RaceDay to enter events and track their participation.

A Participant can:

- Register and log in.
- Manage their personal profile.
- Browse available events.
- View event categories.
- Enrol in an event and select a category.
- View personal event enrolments.
- Cancel eligible enrolments.
- View personal race results.

Role-based access is enforced by the API using session authentication.

---

# Part 1 – System Planning and Database Design

Part 1 focused on planning the RaceDay system before implementation.

Planning documents are stored in the `/docs` folder.

These include:

- `ERD.png` – Entity Relationship Diagram.
- `RaceDay_API_Endpoint_Plan.md` – planned REST API endpoints.
- `RaceDay_Database.sql` – SQL Server database script.
- `RaceDay_CI_Success.png` – evidence of the successful Part 1 GitHub Actions workflow.
- Supporting Part 1 documentation.

---

# Part 2 – ASP.NET Core REST API

Part 2 implements the RaceDay backend as an ASP.NET Core REST API using .NET 8.

The API provides functionality for authentication, user profiles, event management, event categories, participant enrolments and race results.

## Technologies Used

- C#
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- SQL Server / LocalDB
- EF Core Code First Migrations
- Swagger / OpenAPI
- xUnit
- GitHub
- GitHub Actions

---

# Database Design

The RaceDay database contains six main entities:

- Users
- UserProfiles
- Events
- Categories
- Enrollments
- Results

The `Users` table stores both Organisers and Participants. The `Role` field determines which functionality the user is authorised to access.

Each Event belongs to an Organiser.

Each Category belongs to a specific Event.

An Enrollment connects a Participant to an Event and records the category selected by the Participant.

A completed Enrollment may have one Result.

---

# Entity Framework Core

Part 2 uses Entity Framework Core Code First.

The database structure is generated from the C# entity models and `RaceDayDbContext`.

Migration files are stored in:

`/Migrations`

The initial migration creates the RaceDay database structure, relationships, constraints and indexes.

The Part 2 development database is:

`RaceDayPart2DB`

The application uses SQL Server LocalDB during development.

---

# Authentication and Security

RaceDay uses session-based authentication.

When a user successfully logs in, the API stores the following information in the session:

- UserID
- Role

Passwords are not stored as plain text.

ASP.NET Core `PasswordHasher<User>` is used to securely hash passwords during registration and verify passwords during login.

Protected endpoints verify both the user's login session and role before allowing access.

For example:

- Only an Organiser can create an event.
- Only the Organiser who created an event can update or delete that event.
- Only Participants can enrol in events.
- Participants may only cancel their own enrolments.
- Only an event's Organiser may capture results for that event.

---

# API Endpoints

## Authentication

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Register a new Organiser or Participant |
| POST | `/api/auth/login` | Log in and create a session |
| POST | `/api/auth/logout` | Clear the current session |
| GET | `/api/auth/me` | View the currently logged-in user |

## Profile

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/profile` | View the logged-in user's profile |
| PUT | `/api/profile` | Create or update the user's profile |

## Events

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/events` | View all events |
| GET | `/api/events/{id}` | View a specific event |
| POST | `/api/events` | Create an event |
| PUT | `/api/events/{id}` | Update an event |
| DELETE | `/api/events/{id}` | Delete an eligible event |

## Categories

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/events/{eventId}/categories` | View categories belonging to an event |
| POST | `/api/events/{eventId}/categories` | Create a category for an event |
| GET | `/api/categories/{id}` | View a category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete an eligible category |

## Enrollments

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/events/{eventId}/enrollments` | Participant enrols in an event |
| GET | `/api/enrollments/me` | Participant views personal enrolments |
| GET | `/api/events/{eventId}/enrollments` | Organiser views event enrolments |
| DELETE | `/api/enrollments/{id}` | Participant cancels an enrolment |

## Results

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/enrollments/{enrollmentId}/result` | Organiser captures a result |
| PUT | `/api/results/{id}` | Organiser updates a result |
| GET | `/api/events/{eventId}/results` | View results for an event |
| GET | `/api/results/me` | Participant views personal results |

---

# Swagger Documentation

Swagger is enabled in the development environment.

When the API is running, Swagger provides an interactive interface that can be used to view and test the RaceDay endpoints.

The Swagger interface displays the available authentication, profile, event, category, enrollment and result endpoints together with their request schemas.

---

# Running the Project

1. Clone or download the RaceDay repository.
2. Open `Raceday.API.sln` in Visual Studio 2022.
3. Ensure the .NET 8 SDK is installed.
4. Restore the NuGet packages.
5. Confirm that SQL Server LocalDB is available.
6. Check the `RaceDayConnection` connection string in `appsettings.json`.
7. Apply the Entity Framework Core migration if the database has not yet been created.
8. Build the solution.
9. Run the `Raceday.API` project.
10. Swagger should open automatically in the browser.

To apply the migration from Visual Studio Package Manager Console:

`Update-Database`

---

# Automated Testing

The solution contains the `RaceDay.Tests` xUnit test project.

Automated tests cover important functionality including:

- Successful user registration.
- Duplicate email prevention.
- Invalid role rejection.
- Successful login and session creation.
- Incorrect password rejection.
- Participant role restrictions.
- Organiser event creation.
- Participant event enrolment.
- Organiser enrolment restrictions.

The tests use Entity Framework Core's InMemory database provider so that automated tests do not modify the real RaceDay development database.

---

# Continuous Integration

GitHub Actions is used for Continuous Integration.

The Part 2 workflow is located at:

`.github/workflows/part2-ci.yml`

The workflow automatically performs the following actions when changes are pushed to the `main` branch:

1. Checks out the repository.
2. Configures .NET 8.
3. Restores NuGet dependencies.
4. Builds the RaceDay solution.
5. Runs the automated xUnit tests.

The latest RaceDay Part 2 CI workflow successfully completes the build and automated test process.

---

# Repository Structure

The repository contains both Part 1 planning documentation and the Part 2 implementation.

Important locations include:

`/docs` – Part 1 planning and database documentation.

`/Models` – Entity Framework Core models.

`/DTOs` – API request data transfer objects.

`/Data` – database context and project data components.

`/Migrations` – Entity Framework Core migrations.

`/RaceDay.Tests` – automated xUnit tests.

`/.github/workflows` – GitHub Actions CI workflows.

`Raceday.API.sln` – Visual Studio solution.

---

# Validation and Error Handling

The API performs validation for important business rules.

Examples include:

- Duplicate email addresses are rejected.
- Roles must be either `Organiser` or `Participant`.
- Event types must be `Run`, `Walk` or `Cycle`.
- Event capacity cannot be exceeded.
- A Participant cannot enrol in the same active event more than once.
- A selected category must belong to the selected event.
- Participants cannot access Organiser-only functionality.
- Organisers cannot enrol as Participants.
- Results cannot be recorded for cancelled enrolments.
- Duplicate results for the same enrolment are prevented.

Appropriate HTTP status codes such as `200`, `201`, `400`, `401`, `403`, `404` and `409` are returned depending on the outcome of the request.

---

# Part 2 Video Presentation

The Part 2 demonstration presents:

- The RaceDay GitHub repository.
- ASP.NET Core project structure.
- Entity Framework Core models and migrations.
- Swagger API documentation.
- Registration and login functionality.
- Organiser functionality.
- Participant functionality.
- Role-based restrictions.
- Automated xUnit tests.
- Successful GitHub Actions CI workflow.

**Part 2 Video Link:** To be added after the presentation is uploaded.

---

# AI Tool Disclosure

AI tools were used to assist with planning, code review, troubleshooting, documentation and proofreading during development.

The final project structure, implementation, database execution, API testing, automated testing and submission were reviewed by the student.
