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
}