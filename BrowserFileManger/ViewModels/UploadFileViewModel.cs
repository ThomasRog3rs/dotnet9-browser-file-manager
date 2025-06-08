using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using BrowserFileManger.Attributes;
namespace BrowserFileManger.ViewModels;

public class UploadFileViewModel
{
    [Required(ErrorMessage = "Please select a file")]
    [AllowedFileExtensions([".wav", ".mp3"])]
    public IFormFile File { get; set; }
}