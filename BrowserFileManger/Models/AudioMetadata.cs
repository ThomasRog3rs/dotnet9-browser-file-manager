namespace BrowserFileManger.Models;

public class AudioMetadata
{
    public string FileName { get; set; }
    public string Title { get; set; }
    public uint TrackNumber { get; set; }
    public string Album { get; set; }
    public string[] Artists { get; set; }
    public byte[] AlbumArt { get; set; }

    public string AlbumArtBase64
    {
        get
        {
            if(AlbumArt == null) return null;
            var base64 = Convert.ToBase64String(AlbumArt);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}