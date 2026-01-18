using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrowserFileManger.Models;

public class Track
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Title { get; set; }
    
    public uint TrackNumber { get; set; }
    
    public int? AlbumId { get; set; }
    
    [ForeignKey(nameof(AlbumId))]
    public Album? Album { get; set; }
    
    public byte[]? AlbumArtData { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Many-to-many relationship with Artists
    public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
    
    [NotMapped]
    public string? AlbumArtBase64
    {
        get
        {
            if (AlbumArtData == null || AlbumArtData.Length == 0) return null;
            var base64 = Convert.ToBase64String(AlbumArtData);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
    
    [NotMapped]
    public IEnumerable<Artist> Artists => TrackArtists.Select(ta => ta.Artist!);
}
