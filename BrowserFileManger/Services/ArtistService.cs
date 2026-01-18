using Microsoft.EntityFrameworkCore;
using BrowserFileManger.Data;
using BrowserFileManger.Models;

namespace BrowserFileManger.Services;

public class ArtistService
{
    private readonly ApplicationDbContext _context;

    public ArtistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Artist>> GetAllArtistsAsync()
    {
        return await _context.Artists
            .Include(a => a.TrackArtists)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        return await _context.Artists
            .Include(a => a.TrackArtists)
                .ThenInclude(ta => ta.Track)
                    .ThenInclude(t => t!.Album)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Artist?> GetArtistByNameAsync(string name)
    {
        return await _context.Artists
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task<Artist> CreateArtistAsync(string name, string? bio = null)
    {
        // Check for uniqueness
        var existing = await GetArtistByNameAsync(name);
        if (existing != null)
            throw new InvalidOperationException($"Artist with name '{name}' already exists");

        var artist = new Artist
        {
            Name = name,
            Bio = bio,
            CreatedAt = DateTime.UtcNow
        };

        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        return artist;
    }

    public async Task<Artist> GetOrCreateArtistAsync(string name)
    {
        var existing = await GetArtistByNameAsync(name);
        if (existing != null)
            return existing;
            
        var artist = new Artist
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        return artist;
    }

    public async Task<List<Artist>> GetOrCreateArtistsAsync(IEnumerable<string> names)
    {
        var artists = new List<Artist>();
        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var artist = await GetOrCreateArtistAsync(name.Trim());
            artists.Add(artist);
        }
        return artists;
    }

    public async Task UpdateArtistAsync(int id, string name, string? bio)
    {
        var artist = await _context.Artists.FindAsync(id);
        if (artist == null)
            throw new InvalidOperationException($"Artist with ID {id} not found");

        // Check for name uniqueness if name is being changed
        if (!string.Equals(artist.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await GetArtistByNameAsync(name);
            if (existing != null)
                throw new InvalidOperationException($"Artist with name '{name}' already exists");
        }

        artist.Name = name;
        artist.Bio = bio;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteArtistAsync(int id)
    {
        var artist = await _context.Artists.FindAsync(id);
        if (artist != null)
        {
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
        }
    }
}
