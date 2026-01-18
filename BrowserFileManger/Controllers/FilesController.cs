using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BrowserFileManger.ViewModels;
using BrowserFileManger.Services;
using BrowserFileManger.Models;

namespace BrowserFileManger.Controllers;

public class FilesController : Controller
{
    private readonly FileService _fileService;
    private readonly TrackService _trackService;
    private readonly AlbumService _albumService;
    private readonly ArtistService _artistService;
    private readonly MetadataSyncService _syncService;

    public FilesController(
        FileService fileService,
        TrackService trackService,
        AlbumService albumService,
        ArtistService artistService,
        MetadataSyncService syncService)
    {
        _fileService = fileService;
        _trackService = trackService;
        _albumService = albumService;
        _artistService = artistService;
        _syncService = syncService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Upload()
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
    public async Task<IActionResult> Upload(UploadPageViewModel vm)
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
                
                // Import the newly uploaded file to database with metadata extraction
                await _syncService.ImportFileAsync(file.FileName);
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
            TempData["SuccessMessage"] = $"Successfully uploaded {successCount} file(s)";
        }
        
        if (failureCount > 0)
        {
            TempData["ErrorMessage"] = $"Failed to upload {failureCount} file(s): {string.Join("; ", errors)}";
        }
        
        return RedirectToAction("Upload");
    }

    [HttpGet]
    public async Task<IActionResult> List(string? q, int? album, int? artist, string? sort)
    {
        var albums = await _albumService.GetAllAlbumsAsync();
        var artists = await _artistService.GetAllArtistsAsync();
        var tracks = await _trackService.GetAllTracksAsync(q, album, artist, sort);
        
        var vm = new TrackListViewModel
        {
            Tracks = tracks,
            SearchQuery = q,
            FilterAlbumId = album,
            FilterArtistId = artist,
            SortBy = sort,
            AlbumOptions = new SelectList(albums, "Id", "Name"),
            ArtistOptions = new SelectList(artists, "Id", "Name")
        };
        
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var track = await _trackService.GetTrackByIdAsync(id);
        if (track == null)
            return NotFound();

        var albums = await _albumService.GetAllAlbumsAsync();
        var artists = await _artistService.GetAllArtistsAsync();

        var vm = new EditTrackViewModel
        {
            Id = track.Id,
            FileName = track.FileName,
            Title = track.Title,
            TrackNumber = track.TrackNumber,
            AlbumId = track.AlbumId,
            SelectedArtistIds = track.TrackArtists.Select(ta => ta.ArtistId).ToList(),
            CurrentAlbumArtBase64 = track.AlbumArtBase64,
            AlbumOptions = new SelectList(albums, "Id", "Name"),
            ArtistOptions = new MultiSelectList(artists, "Id", "Name", track.TrackArtists.Select(ta => ta.ArtistId)),
            ArtistSuggestions = artists.Select(a => a.Name).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTrackViewModel vm)
    {
        vm.SelectedArtistIds ??= new();
        ModelState.Remove(nameof(EditTrackViewModel.SelectedArtistIds));

        if (!ModelState.IsValid)
        {
            var albums = await _albumService.GetAllAlbumsAsync();
            var artists = await _artistService.GetAllArtistsAsync();
            vm.AlbumOptions = new SelectList(albums, "Id", "Name");
            vm.ArtistOptions = new MultiSelectList(artists, "Id", "Name", vm.SelectedArtistIds);
            vm.ArtistSuggestions = artists.Select(a => a.Name).ToList();
            return View(vm);
        }

        var track = await _trackService.GetTrackByIdAsync(vm.Id);
        if (track == null)
            return NotFound();

        // Handle new album creation
        int? albumId = vm.AlbumId;
        if (!string.IsNullOrWhiteSpace(vm.NewAlbumName))
        {
            var newAlbum = await _albumService.GetOrCreateAlbumAsync(vm.NewAlbumName.Trim());
            albumId = newAlbum.Id;
        }

        // Handle new artist creation
        var artistIds = vm.SelectedArtistIds ?? new List<int>();
        if (!string.IsNullOrWhiteSpace(vm.NewArtistNames))
        {
            var newArtistNames = vm.NewArtistNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var newArtists = await _artistService.GetOrCreateArtistsAsync(newArtistNames);
            artistIds.AddRange(newArtists.Select(a => a.Id));
            artistIds = artistIds.Distinct().ToList();
        }

        // Handle album art
        byte[]? albumArt = null;
        if (vm.AlbumArtFile != null && vm.AlbumArtFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await vm.AlbumArtFile.CopyToAsync(ms);
            albumArt = ms.ToArray();
        }

        await _trackService.UpdateTrackAsync(
            vm.Id,
            vm.Title,
            vm.TrackNumber,
            albumId,
            artistIds,
            albumArt,
            vm.RemoveAlbumArt);

        TempData["SuccessMessage"] = "Track metadata updated successfully!";
        return RedirectToAction("List");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var track = await _trackService.GetTrackByIdAsync(id);
        if (track == null)
            return NotFound();

        // Delete file from disk
        var filePath = _fileService.GetFilePath(track.FileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        // Delete from database
        await _trackService.DeleteTrackAsync(id);

        TempData["SuccessMessage"] = "Track deleted successfully!";
        return RedirectToAction("List");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync()
    {
        var (imported, removed) = await _syncService.FullSyncAsync();
        TempData["SuccessMessage"] = $"Sync complete: {imported} files imported, {removed} orphaned records removed.";
        return RedirectToAction("List");
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
