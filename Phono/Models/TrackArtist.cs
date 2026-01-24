namespace Phono.Models;

public class TrackArtist
{
    public int TrackId { get; set; }
    public Track? Track { get; set; }
    
    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }
}
