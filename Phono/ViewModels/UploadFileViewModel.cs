using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Phono.Attributes;
namespace Phono.ViewModels;

public class UploadFileViewModel
{
    [Required(ErrorMessage = "Please select at least one file")]
    [AllowedFileExtensions([".wav", ".mp3", ".flac"])]
    public List<IFormFile> Files { get; set; } = new();
}