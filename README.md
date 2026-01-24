# Phono

A simple ASP.NET Core app for managing local audio files and metadata.

## Prerequisites

### FFmpeg (Required for Audio Compression)

This application uses FFmpeg for audio compression. FFmpeg must be installed and available in your system PATH.

**macOS (Homebrew):**
```bash
brew install ffmpeg
```

**Ubuntu/Debian:**
```bash
sudo apt update && sudo apt install ffmpeg
```

**Windows (Chocolatey):**
```bash
choco install ffmpeg
```

**Windows (Manual):**
1. Download from https://ffmpeg.org/download.html
2. Extract to a folder (e.g., `C:\ffmpeg`)
3. Add the `bin` folder to your system PATH

Verify installation:
```bash
ffmpeg -version
```

## Quick start

```bash
dotnet run --project Phono
```

## Docker

Build and run:

```bash
docker compose up --build
```

Then open `http://localhost:8080`.

Docker uses SQLite with a local `./data` directory for the database and `./uploads`
for audio files, so your data persists across container restarts.

## First pull: seed the database schema

The SQLite database file is not tracked in git. After your first pull, run the
seed script to create the schema:

- macOS/Linux: `./scripts/seed-db.sh`
- PowerShell: `./scripts/seed-db.ps1`

This runs EF Core migrations and exits.

## Audio Compression

Uploaded audio files are automatically compressed to save storage space:

- **Lossless formats** (WAV, FLAC) are converted to MP3
- **High-bitrate MP3 files** (>192kbps) are recompressed to 192kbps
- Original files are replaced with compressed versions

Compression settings can be configured in `appsettings.json`:

```json
{
  "Compression": {
    "Enabled": true,
    "TargetBitrate": 192,
    "RecompressThreshold": 192,
    "TargetFormat": "mp3",
    "DeleteOriginals": true
  }
}
```
