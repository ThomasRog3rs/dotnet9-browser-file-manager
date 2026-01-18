using System.ComponentModel.DataAnnotations;

namespace BrowserFileManger.Models;

public class Artist
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Bio { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Many-to-many relationship with Tracks
    public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
}
