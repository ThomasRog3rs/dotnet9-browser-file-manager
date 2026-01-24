using Phono.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Phono.ViewModels;

public class TrackListViewModel
{
    public List<Track> Tracks { get; set; } = new();
    
    // Filter options
    public string? SearchQuery { get; set; }
    public int? FilterAlbumId { get; set; }
    public int? FilterArtistId { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    
    // Dropdown options for filters
    public SelectList? AlbumOptions { get; set; }
    public SelectList? ArtistOptions { get; set; }
    
    public static readonly Dictionary<string, string> SortOptions = new()
    {
        { "title", "Title (A-Z)" },
        { "title_desc", "Title (Z-A)" },
        { "album", "Album (A-Z)" },
        { "album_desc", "Album (Z-A)" },
        { "artist", "Artist (A-Z)" },
        { "artist_desc", "Artist (Z-A)" },
        { "track", "Track Number" },
        { "date", "Date Added (Newest)" },
        { "date_asc", "Date Added (Oldest)" }
    };
}
