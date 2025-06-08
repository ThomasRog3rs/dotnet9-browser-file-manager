using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace BrowserFileManger.ViewModels;

public class UplodaFileViewModel
{
    [Required(ErrorMessage = "Please select a file")]
    public required IFormFile File { get; set; }
}