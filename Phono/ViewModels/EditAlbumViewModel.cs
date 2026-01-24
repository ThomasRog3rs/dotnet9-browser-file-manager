using System.ComponentModel.DataAnnotations;

namespace Phono.ViewModels;

public class EditAlbumViewModel
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    [Display(Name = "Album Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Release Year")]
    [Range(1900, 2100)]
    public int? ReleaseYear { get; set; }
    
    [Display(Name = "Album Artwork")]
    public IFormFile? AlbumArtFile { get; set; }
    
    public string? CurrentAlbumArtBase64 { get; set; }
    
    public bool RemoveAlbumArt { get; set; }
    
    public int TrackCount { get; set; }
}
