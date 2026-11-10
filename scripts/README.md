# RaceDay Part 3 — Codespaces database and login test

This project originally used SQL Server **LocalDB**, which works on Windows but **not** in Linux Codespaces. Its MVC `appsettings.json` also pointed to the previous Windows API port (`https://localhost:7276/`).

## How to use in GitHub Codespaces

1. Use the repository root (`/workspaces/RaceDay`). Stop any currently running RaceDay applications (Ctrl+C in their respective terminal tabs).
2. Run `bash scripts/setup-codespaces.sh`. It creates a **local SQL Server 2022 Docker container**, a persistent database volume, a gitignored local password file and applies the original Entity Framework SQL Server migrations.
3. Open a **new terminal** and run `bash scripts/run-api.sh`. Leave it running (port 5081).
4. Open a **second new terminal** and run `bash scripts/run-web.sh`. Leave it running (port 5080).
5. Open a **third terminal** and run `python3 scripts/smoke-test.py`. The smoke test creates random *test accounts* for Participant and Organiser, submits the MVC registration and login forms and checks the correct dashboard and profile access.
6. Open the **Ports** tab and open **5080** in the browser to see the website.

The SQL Server database is persistent in the `raceday-part3-sql-data` Docker volume. The password is stored locally in `.raceday-codespaces.env` and excluded by `.gitignore` — **never commit it**.

If the database doesn't start, check `docker logs raceday-part3-sql`. If EF complains the command isn't found, run `bash scripts/setup-codespaces.sh` again. The setup requires .NET 8, Docker and network access to download the SQL Server image and EF tool if not already present.

If port 5080 or 5081 is in use, stop the old RaceDay process in that terminal before starting these scripts.

**Azure image uploads** are separate from database/login testing and require Azure Blob Storage credentials configured via environment variables or user secrets; this test does not certify those uploads.

**Note:** The provided ZIP was statically audited here, but .NET and Docker were not available to execute the C# build and live integration test in the review environment. Execute the scripts in Codespaces to confirm pass/fail, and keep any errors for diagnosis.
