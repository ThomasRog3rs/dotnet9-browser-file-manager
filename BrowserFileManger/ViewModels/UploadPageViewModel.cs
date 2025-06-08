using BrowserFileManger.Models;

namespace BrowserFileManger.ViewModels;

public class UploadPageViewModel
{
    public UploadFileViewModel FileUpload { get; set; } = new();
    public List<AudioMetadata> Files {get; set;} = new();
}