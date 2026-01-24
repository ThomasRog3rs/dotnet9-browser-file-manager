using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Phono.ViewModels;

public class EditTrackViewModel
{
    public int Id { get; set; }
    
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    [Display(Name = "Title")]
    public string? Title { get; set; }
    
    [Display(Name = "Track Number")]
    [Range(0, 9999)]
    public uint TrackNumber { get; set; }
    
    [Display(Name = "Album")]
    public int? AlbumId { get; set; }
    
    [Display(Name = "New Album Name")]
    public string? NewAlbumName { get; set; }
    
    [Display(Name = "Artists")]
public List<int>? SelectedArtistIds { get; set; } = new();
    
    [Display(Name = "New Artist Names (comma-separated)")]
    public string? NewArtistNames { get; set; }
    
    [Display(Name = "Album Artwork")]
    public IFormFile? AlbumArtFile { get; set; }
    
    public string? CurrentAlbumArtBase64 { get; set; }
    
    public bool RemoveAlbumArt { get; set; }
    
    // Dropdown options
    public SelectList? AlbumOptions { get; set; }
    public MultiSelectList? ArtistOptions { get; set; }

    public List<string> ArtistSuggestions { get; set; } = new();
}
