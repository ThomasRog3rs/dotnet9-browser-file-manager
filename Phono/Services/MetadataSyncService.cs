using Microsoft.EntityFrameworkCore;
using Phono.Data;
using Phono.Models;

namespace Phono.Services;

public class MetadataSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly FileService _fileService;
    private readonly AlbumService _albumService;
    private readonly ArtistService _artistService;

    public MetadataSyncService(
        ApplicationDbContext context, 
        FileService fileService,
        AlbumService albumService,
        ArtistService artistService)
    {
        _context = context;
        _fileService = fileService;
        _albumService = albumService;
        _artistService = artistService;
    }

    /// <summary>
    /// Scans the uploads folder and imports any files not already in the database
    /// </summary>
    public async Task<int> ImportNewFilesAsync()
    {
        var fileNames = _fileService.GetFileNames();
        var existingFileNames = await _context.Tracks
            .Select(t => t.FileName)
            .ToListAsync();

        var newFiles = fileNames.Where(f => !existingFileNames.Contains(f)).ToList();
        int importedCount = 0;

        foreach (var fileName in newFiles)
        {
            try
            {
                await ImportFileAsync(fileName);
                importedCount++;
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                Console.WriteLine($"Error importing {fileName}: {ex.Message}");
            }
        }

        return importedCount;
    }

    /// <summary>
    /// Imports a single file from the uploads folder into the database
    /// </summary>
    public async Task<Track> ImportFileAsync(string fileName)
    {
        // Check if already exists
        var existing = await _context.Tracks.FirstOrDefaultAsync(t => t.FileName == fileName);
        if (existing != null)
            return existing;

        // Extract metadata from file
        var (title, trackNumber, albumName, artistNames, albumArt) = _fileService.ExtractRawMetadata(fileName);

        // Create or get album
        Album? album = null;
        if (!string.IsNullOrWhiteSpace(albumName))
        {
            album = await _albumService.GetOrCreateAlbumAsync(albumName);
        }

        // Create or get artists
        var artists = new List<Artist>();
        if (artistNames != null && artistNames.Length > 0)
        {
            artists = await _artistService.GetOrCreateArtistsAsync(artistNames);
        }

        // Create track
        var track = new Track
        {
            FileName = fileName,
            FilePath = _fileService.GetFilePath(fileName),
            Title = title,
            TrackNumber = trackNumber,
            AlbumId = album?.Id,
            AlbumArtData = albumArt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        // Add artist relationships
        foreach (var artist in artists)
        {
            _context.TrackArtists.Add(new TrackArtist
            {
                TrackId = track.Id,
                ArtistId = artist.Id
            });
        }
        await _context.SaveChangesAsync();

        return track;
    }

    /// <summary>
    /// Removes tracks from database that no longer have corresponding files
    /// </summary>
    public async Task<int> CleanupMissingFilesAsync()
    {
        var tracks = await _context.Tracks.ToListAsync();
        var removedCount = 0;

        foreach (var track in tracks)
        {
            if (!_fileService.FileExists(track.FileName))
            {
                _context.Tracks.Remove(track);
                removedCount++;
            }
        }

        await _context.SaveChangesAsync();
        return removedCount;
    }

    /// <summary>
    /// Full sync: import new files and cleanup missing ones
    /// </summary>
    public async Task<(int Imported, int Removed)> FullSyncAsync()
    {
        var imported = await ImportNewFilesAsync();
        var removed = await CleanupMissingFilesAsync();
        return (imported, removed);
    }
}
