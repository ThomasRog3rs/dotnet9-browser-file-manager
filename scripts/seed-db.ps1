$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "..\Phono"
dotnet run --project $project -- --migrate-only
