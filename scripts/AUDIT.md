# RaceDay Part 3 review — 10 November 2026

## Source files inspected

- `Raceday part 3/Raceday.API.csproj` and the MVC project under `Raceday part 3/RaceDay.Web`
- API authentication controller and both request DTOs
- MVC account controller and the API HTTP client/session cookie forwarding code
- EF Core `RaceDayDbContext`, migrations, and SQL Server connection configuration
- MVC registration/login pages, role filters and media integration
- CI workflows and xUnit test file presence

## Confirmed from the uploaded code

1. The API's `appsettings.json` contains `(localdb)\\MSSQLLocalDB`. LocalDB is Windows-only, so database queries from a Linux Codespace cannot connect to that configured server.
2. The MVC `appsettings.json` pointed at `https://localhost:7276/`, while the API was reported running on `http://localhost:5081/` in the user's Codespace. This mismatch causes the MVC "API not reachable" message unless overridden by environment configuration.
3. Registration and login endpoints exist, and MVC has the HTML forms, server-side session forwarding code and role-based dashboard redirects. This presence does not establish that they run successfully.
4. Azure Blob Storage configuration is not present in the uploaded API configuration. Image upload functionality requires separately configured credentials; none have been generated or guessed.

## Applied changes

- MVC default API address changed to `http://localhost:5081/`.
- Added a Codespaces-specific SQL Server Docker setup, per-terminal API/website run commands, a real HTTP registration/login smoke test and a `.gitignore` to keep the generated SQL password untracked.
- Added GitHub Actions Part 3 workflow for .NET 8 build, automated tests, SQL Server migrations and MVC end-to-end login tests.
- No changes were made to original root-level Part 2 code, controller/business logic or Windows LocalDB default configuration.

## Verification limits

The uploaded source was inspected and new JSON, Python and shell files were syntax-checked. This evaluation environment does not contain the .NET SDK or Docker, and external package downloads are unavailable. As a result, C# compilation, migrations, test results, the browser and Azure uploads **have not been run here**. After applying the patch to GitHub, the new Part 3 GitHub Actions workflow and the Codespaces smoke-test script provide observable pass/fail results.
