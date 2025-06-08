namespace BrowserFileManger.ViewModels;

public class UploadPageViewModel
{
    public UploadFileViewModel FileUpload { get; set; } = new();
    public List<string> Files {get; set;} = new();
}