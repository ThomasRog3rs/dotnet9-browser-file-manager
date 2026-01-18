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
    private readonly TrackService _trackService;
    private readonly MetadataSyncService _syncService;

    public HomeController(
        ILogger<HomeController> logger, 
        FileService fileService,
        TrackService trackService,
        MetadataSyncService syncService)
    {
        _logger = logger;
        _fileService = fileService;
        _trackService = trackService;
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tracks = await _trackService.GetAllTracksAsync();
        var vm = new UploadPageViewModel
        {
            FileUpload = new UploadFileViewModel(),
            Files = tracks.Select(TrackToAudioMetadata).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Index(UploadPageViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var tracks = await _trackService.GetAllTracksAsync();
            vm.Files = tracks.Select(TrackToAudioMetadata).ToList();
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
        
        // Import the newly uploaded file to database
        await _syncService.ImportFileAsync(file.FileName);
        
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
    
    // Helper method to convert Track to AudioMetadata for legacy view compatibility
    private static AudioMetadata TrackToAudioMetadata(Track track)
    {
        return new AudioMetadata
        {
            FileName = track.FileName,
            Title = track.Title,
            TrackNumber = track.TrackNumber,
            Album = track.Album?.Name,
            Artists = track.TrackArtists.Select(ta => ta.Artist?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToArray(),
            AlbumArt = track.AlbumArtData
        };
    }
}
