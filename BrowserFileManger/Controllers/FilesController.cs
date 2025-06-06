using Microsoft.AspNetCore.Mvc;

namespace BrowserFileManger.Controllers;

public class FilesController : Controller
{
    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file");
            return View();
        }
        
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
        
        var files = Directory.GetFiles(uploadsPath).Select(f => Path.GetFileName(f)).ToList();
        
        return View(files);
        
    }

}