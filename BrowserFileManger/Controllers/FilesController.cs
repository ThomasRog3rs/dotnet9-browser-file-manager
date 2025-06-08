using Microsoft.AspNetCore.Mvc;
using BrowserFileManger.ViewModels;
using BrowserFileManger.Services;
namespace BrowserFileManger.Controllers;

public class FilesController : Controller
{
    private readonly string _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    [HttpGet]
    public IActionResult Upload()
    {
        var files = FileService.GetFiles(_uploadsPath);   
        var vm = new UploadPageViewModel
        {
            FileUpload = new UploadFileViewModel(),
            Files = files
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(UploadPageViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var files = FileService.GetFiles(_uploadsPath);   
            vm.Files = files;
            return View(vm);
        }

        var file = vm.FileUpload.File;
        
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);
        
        var filePath = Path.Combine(uploads, file.FileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        return RedirectToAction("Upload");
    }

    [HttpGet]
    public IActionResult List()
    {
        var files = FileService.GetFiles(_uploadsPath);   
        return View(files);
    }

}