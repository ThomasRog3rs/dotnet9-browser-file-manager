# Browser File Manager

A simple ASP.NET Core app for managing local audio files and metadata.

## Quick start

```bash
dotnet run --project BrowserFileManger
```

## First pull: seed the database schema

The SQLite database file is not tracked in git. After your first pull, run the
seed script to create the schema:

- macOS/Linux: `./scripts/seed-db.sh`
- PowerShell: `./scripts/seed-db.ps1`

This runs EF Core migrations and exits.
