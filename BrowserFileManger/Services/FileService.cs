using BrowserFileManger.Models; 
using TagLib;
namespace BrowserFileManger.Services;

public class FileService
{
    private readonly string _uploadsPath;

    public FileService()
    {
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if(!Directory.Exists(_uploadsPath))
            Directory.CreateDirectory(_uploadsPath);
    }
    
    public string UploadsPath => _uploadsPath;
    
    public List<string> GetFileNames()
    {
        return Directory.GetFiles(_uploadsPath)
            .Select(file => Path.GetFileName(file))
            .ToList();
    }

    public List<AudioMetadata> GetFilesWithMetaData()
    {
        var fileNames = GetFileNames();
        List<AudioMetadata> audioMetadata = new List<AudioMetadata>();
        foreach (var fileName in fileNames)
        {
            var fileMetaData = GetAudioMetadata(fileName);
            audioMetadata.Add(fileMetaData);
        }
         
        return audioMetadata;
    }
    
    public AudioMetadata GetAudioMetadata(string fileName)
    {
        var filePath = Path.Combine(_uploadsPath, fileName);
        var file = TagLib.File.Create(filePath);

        var metadata = new AudioMetadata
        {
            FileName = fileName,
            Title = file.Tag.Title,
            TrackNumber = file.Tag.Track,
            Album = file.Tag.Album,
            Artists = file.Tag.AlbumArtists,
            AlbumArt = file.Tag.Pictures.FirstOrDefault()?.Data.Data
        };
        
        return metadata;
    }
    
    public string GetFilePath(string fileName)
    {
        return Path.Combine(_uploadsPath, fileName);
    }
    
    public bool FileExists(string fileName)
    {
        return System.IO.File.Exists(GetFilePath(fileName));
    }
    
    /// <summary>
    /// Updates the metadata in an audio file using TagLibSharp
    /// </summary>
    public void UpdateFileMetadata(string fileName, string? title, uint trackNumber, string? albumName, string[]? artistNames, byte[]? albumArt, bool removeArt = false)
    {
        var filePath = GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Audio file not found: {fileName}");
            
        using var file = TagLib.File.Create(filePath);
        
        file.Tag.Title = title;
        file.Tag.Track = trackNumber;
        file.Tag.Album = albumName;
        
        // Set both Performers and AlbumArtists for maximum compatibility
        if (artistNames != null && artistNames.Length > 0)
        {
            file.Tag.Performers = artistNames;
            file.Tag.AlbumArtists = artistNames;
        }
        else
        {
            file.Tag.Performers = Array.Empty<string>();
            file.Tag.AlbumArtists = Array.Empty<string>();
        }
        
        // Handle album art
        if (removeArt)
        {
            file.Tag.Pictures = Array.Empty<IPicture>();
        }
        else if (albumArt != null && albumArt.Length > 0)
        {
            var picture = new Picture(new ByteVector(albumArt))
            {
                Type = PictureType.FrontCover,
                MimeType = "image/jpeg",
                Description = "Album Art"
            };
            file.Tag.Pictures = new IPicture[] { picture };
        }
        
        file.Save();
    }
    
    /// <summary>
    /// Extracts raw metadata from a file for import purposes
    /// </summary>
    public (string? Title, uint TrackNumber, string? Album, string[]? Artists, byte[]? AlbumArt) ExtractRawMetadata(string fileName)
    {
        var filePath = GetFilePath(fileName);
        if (!System.IO.File.Exists(filePath))
            return (null, 0, null, null, null);
            
        using var file = TagLib.File.Create(filePath);
        
        var artists = file.Tag.AlbumArtists?.Length > 0 
            ? file.Tag.AlbumArtists 
            : file.Tag.Performers;
            
        var albumArt = file.Tag.Pictures.FirstOrDefault()?.Data.Data;
        
        return (file.Tag.Title, file.Tag.Track, file.Tag.Album, artists, albumArt);
    }
}