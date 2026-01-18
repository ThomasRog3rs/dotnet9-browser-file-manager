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
    private readonly AudioCompressionService _compressionService;

    public HomeController(
        ILogger<HomeController> logger, 
        FileService fileService,
        TrackService trackService,
        MetadataSyncService syncService,
        AudioCompressionService compressionService)
    {
        _logger = logger;
        _fileService = fileService;
        _trackService = trackService;
        _syncService = syncService;
        _compressionService = compressionService;
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

        var files = vm.FileUpload.Files;
        if (files == null || !files.Any())
        {
            ModelState.AddModelError("", "Please select at least one file to upload");
            var tracks = await _trackService.GetAllTracksAsync();
            vm.Files = tracks.Select(TrackToAudioMetadata).ToList();
            return View(vm);
        }
        
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);
        
        int successCount = 0;
        int failureCount = 0;
        long totalBytesSaved = 0;
        int filesCompressed = 0;
        var errors = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var filePath = Path.Combine(uploads, file.FileName);
                
                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Compress the file if needed
                var compressionResult = await _compressionService.CompressIfNeededAsync(file.FileName);
                var finalFileName = compressionResult.CompressedFileName;
                
                if (compressionResult.WasCompressed)
                {
                    totalBytesSaved += compressionResult.BytesSaved;
                    filesCompressed++;
                }
                
                // Import the file to database with metadata extraction
                await _syncService.ImportFileAsync(finalFileName);
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                errors.Add($"{file.FileName}: {ex.Message}");
            }
        }

        // Provide feedback to user
        if (successCount > 0)
        {
            var message = $"Successfully uploaded {successCount} file(s)";
            if (filesCompressed > 0)
            {
                message += $". Compressed {filesCompressed} file(s), saved {AudioCompressionService.FormatBytes(totalBytesSaved)}";
            }
            TempData["SuccessMessage"] = message;
        }
        
        if (failureCount > 0)
        {
            TempData["ErrorMessage"] = $"Failed to upload {failureCount} file(s): {string.Join("; ", errors)}";
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
