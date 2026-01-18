using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BrowserFileManger.Models;
using BrowserFileManger.Services;
using BrowserFileManger.ViewModels;

namespace BrowserFileManger.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FileService _fileService;

    public HomeController(ILogger<HomeController> logger, FileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var files = _fileService.GetFilesWithMetaData();
        var vm = new UploadPageViewModel
        {
            FileUpload = new UploadFileViewModel(),
            Files = files
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Index(UploadPageViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var files = _fileService.GetFilesWithMetaData();
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
        
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}