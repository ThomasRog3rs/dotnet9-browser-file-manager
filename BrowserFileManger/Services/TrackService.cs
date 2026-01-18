using Microsoft.EntityFrameworkCore;
using BrowserFileManger.Data;
using BrowserFileManger.Models;

namespace BrowserFileManger.Services;

public class TrackService
{
    private readonly ApplicationDbContext _context;
    private readonly FileService _fileService;

    public TrackService(ApplicationDbContext context, FileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<List<Track>> GetAllTracksAsync(
        string? searchQuery = null,
        int? albumId = null,
        int? artistId = null,
        string? sortBy = null)
    {
        var query = _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchLower = searchQuery.ToLower();
            query = query.Where(t => 
                (t.Title != null && t.Title.ToLower().Contains(searchLower)) ||
                t.FileName.ToLower().Contains(searchLower));
        }

        // Apply album filter
        if (albumId.HasValue)
        {
            query = query.Where(t => t.AlbumId == albumId.Value);
        }

        // Apply artist filter
        if (artistId.HasValue)
        {
            query = query.Where(t => t.TrackArtists.Any(ta => ta.ArtistId == artistId.Value));
        }

        // Apply sorting
        query = sortBy switch
        {
            "title" => query.OrderBy(t => t.Title ?? t.FileName),
            "title_desc" => query.OrderByDescending(t => t.Title ?? t.FileName),
            "album" => query.OrderBy(t => t.Album != null ? t.Album.Name : "").ThenBy(t => t.TrackNumber),
            "album_desc" => query.OrderByDescending(t => t.Album != null ? t.Album.Name : "").ThenBy(t => t.TrackNumber),
            "artist" => query.OrderBy(t => t.TrackArtists.Select(ta => ta.Artist!.Name).FirstOrDefault() ?? ""),
            "artist_desc" => query.OrderByDescending(t => t.TrackArtists.Select(ta => ta.Artist!.Name).FirstOrDefault() ?? ""),
            "track" => query.OrderBy(t => t.Album != null ? t.Album.Name : "").ThenBy(t => t.TrackNumber),
            "date" => query.OrderByDescending(t => t.CreatedAt),
            "date_asc" => query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<Track?> GetTrackByIdAsync(int id)
    {
        return await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Track?> GetTrackByFileNameAsync(string fileName)
    {
        return await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
            .FirstOrDefaultAsync(t => t.FileName == fileName);
    }

    public async Task<Track> CreateTrackAsync(string fileName, string? title, uint trackNumber, 
        int? albumId, List<int> artistIds, byte[]? albumArt)
    {
        var track = new Track
        {
            FileName = fileName,
            FilePath = _fileService.GetFilePath(fileName),
            Title = title,
            TrackNumber = trackNumber,
            AlbumId = albumId,
            AlbumArtData = albumArt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tracks.Add(track);
        await _context.SaveChangesAsync();

        // Add artist relationships
        foreach (var artistId in artistIds)
        {
            _context.TrackArtists.Add(new TrackArtist
            {
                TrackId = track.Id,
                ArtistId = artistId
            });
        }
        await _context.SaveChangesAsync();

        return track;
    }

    public async Task UpdateTrackAsync(int id, string? title, uint trackNumber, 
        int? albumId, List<int> artistIds, byte[]? albumArt, bool removeArt = false)
    {
        var track = await _context.Tracks
            .Include(t => t.TrackArtists)
            .Include(t => t.Album)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (track == null)
            throw new InvalidOperationException($"Track with ID {id} not found");

        track.Title = title;
        track.TrackNumber = trackNumber;
        track.AlbumId = albumId;
        track.UpdatedAt = DateTime.UtcNow;
        
        if (removeArt)
        {
            track.AlbumArtData = null;
        }
        else if (albumArt != null && albumArt.Length > 0)
        {
            track.AlbumArtData = albumArt;
        }

        // Update artist relationships - remove old ones
        _context.TrackArtists.RemoveRange(track.TrackArtists);
        
        // Add new ones
        foreach (var artistId in artistIds)
        {
            _context.TrackArtists.Add(new TrackArtist
            {
                TrackId = track.Id,
                ArtistId = artistId
            });
        }

        await _context.SaveChangesAsync();

        // Sync metadata to file
        await SyncTrackToFileAsync(track);
    }

    public async Task SyncTrackToFileAsync(Track track)
    {
        // Reload track with all related data
        var fullTrack = await _context.Tracks
            .Include(t => t.Album)
            .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
            .FirstOrDefaultAsync(t => t.Id == track.Id);

        if (fullTrack == null) return;

        var artistNames = fullTrack.TrackArtists
            .Where(ta => ta.Artist != null)
            .Select(ta => ta.Artist!.Name)
            .ToArray();

        _fileService.UpdateFileMetadata(
            fullTrack.FileName,
            fullTrack.Title,
            fullTrack.TrackNumber,
            fullTrack.Album?.Name,
            artistNames,
            fullTrack.AlbumArtData
        );
    }

    public async Task DeleteTrackAsync(int id)
    {
        var track = await _context.Tracks.FindAsync(id);
        if (track != null)
        {
            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();
        }
    }
}
