using Microsoft.AspNetCore.Mvc;
using BrowserFileManger.ViewModels;

namespace BrowserFileManger.Controllers;

public class FilesController : Controller
{
    [HttpGet]
    public IActionResult Upload()
    {
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
        
        var files = Directory.GetFiles(uploadsPath)
            .Select(f => Path.GetFileName(f))
            .ToList();
        
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
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);

            var files = Directory.GetFiles(uploadsPath)
                .Select(f => Path.GetFileName(f))
                .ToList();

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
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
        
        var files = Directory.GetFiles(uploadsPath)
            .Select(f => Path.GetFileName(f))
            .ToList();

        return View(files);
    }

}