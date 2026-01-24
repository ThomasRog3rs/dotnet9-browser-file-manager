using System.ComponentModel.DataAnnotations;

namespace Phono.Models;

public class Album
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    public int? ReleaseYear { get; set; }
    
    public byte[]? AlbumArtData { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    
    public string? AlbumArtBase64
    {
        get
        {
            if (AlbumArtData == null || AlbumArtData.Length == 0) return null;
            var base64 = Convert.ToBase64String(AlbumArtData);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}
