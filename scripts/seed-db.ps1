$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "..\BrowserFileManger"
dotnet run --project $project -- --migrate-only
