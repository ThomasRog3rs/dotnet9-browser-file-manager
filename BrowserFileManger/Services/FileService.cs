namespace BrowserFileManger.Services;

public static class FileService
{
    public static List<string> GetFiles(string directoryPath)
    {
        return Directory.GetFiles(directoryPath)
            .Select(file => Path.GetFileName(file))
            .ToList();
    }
}