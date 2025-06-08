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
}