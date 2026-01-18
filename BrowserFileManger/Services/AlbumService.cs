using Microsoft.EntityFrameworkCore;
using BrowserFileManger.Data;
using BrowserFileManger.Models;

namespace BrowserFileManger.Services;

public class AlbumService
{
    private readonly ApplicationDbContext _context;

    public AlbumService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Album>> GetAllAlbumsAsync()
    {
        return await _context.Albums
            .Include(a => a.Tracks)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Album?> GetAlbumByIdAsync(int id)
    {
        return await _context.Albums
            .Include(a => a.Tracks)
                .ThenInclude(t => t.TrackArtists)
                    .ThenInclude(ta => ta.Artist)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Album?> GetAlbumByNameAsync(string name)
    {
        return await _context.Albums
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task<Album> CreateAlbumAsync(string name, int? releaseYear = null, byte[]? albumArt = null)
    {
        var album = new Album
        {
            Name = name,
            ReleaseYear = releaseYear,
            AlbumArtData = albumArt,
            CreatedAt = DateTime.UtcNow
        };

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();
        return album;
    }

    public async Task<Album> GetOrCreateAlbumAsync(string name)
    {
        var existing = await GetAlbumByNameAsync(name);
        if (existing != null)
            return existing;
            
        return await CreateAlbumAsync(name);
    }

    public async Task UpdateAlbumAsync(int id, string name, int? releaseYear, byte[]? albumArt, bool removeArt = false)
    {
        var album = await _context.Albums.FindAsync(id);
        if (album == null)
            throw new InvalidOperationException($"Album with ID {id} not found");

        album.Name = name;
        album.ReleaseYear = releaseYear;
        
        if (removeArt)
        {
            album.AlbumArtData = null;
        }
        else if (albumArt != null && albumArt.Length > 0)
        {
            album.AlbumArtData = albumArt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAlbumAsync(int id)
    {
        var album = await _context.Albums.FindAsync(id);
        if (album != null)
        {
            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
        }
    }
}
