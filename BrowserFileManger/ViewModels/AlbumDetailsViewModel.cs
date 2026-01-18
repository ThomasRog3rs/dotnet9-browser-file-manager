using BrowserFileManger.Models;

namespace BrowserFileManger.ViewModels;

public class AlbumDetailsViewModel
{
    public Album Album { get; set; } = null!;
    public List<Track> Tracks { get; set; } = new();
}
