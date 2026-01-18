using Microsoft.AspNetCore.Mvc;
using BrowserFileManger.ViewModels;
using BrowserFileManger.Services;

namespace BrowserFileManger.Controllers;

public class AlbumsController : Controller
{
    private readonly AlbumService _albumService;

    public AlbumsController(AlbumService albumService)
    {
        _albumService = albumService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var albums = await _albumService.GetAllAlbumsAsync();
        return View(albums);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var album = await _albumService.GetAlbumByIdAsync(id);
        if (album == null)
            return NotFound();

        var tracks = album.Tracks
            .OrderBy(t => t.TrackNumber == 0 ? int.MaxValue : (int)t.TrackNumber)
            .ThenBy(t => t.Title ?? t.FileName)
            .ToList();

        var vm = new AlbumDetailsViewModel
        {
            Album = album,
            Tracks = tracks
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EditAlbumViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditAlbumViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        byte[]? albumArt = null;
        if (vm.AlbumArtFile != null && vm.AlbumArtFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await vm.AlbumArtFile.CopyToAsync(ms);
            albumArt = ms.ToArray();
        }

        await _albumService.CreateAlbumAsync(vm.Name, vm.ReleaseYear, albumArt);
        
        TempData["SuccessMessage"] = "Album created successfully!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var album = await _albumService.GetAlbumByIdAsync(id);
        if (album == null)
            return NotFound();

        var vm = new EditAlbumViewModel
        {
            Id = album.Id,
            Name = album.Name,
            ReleaseYear = album.ReleaseYear,
            CurrentAlbumArtBase64 = album.AlbumArtBase64,
            TrackCount = album.Tracks.Count
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditAlbumViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        byte[]? albumArt = null;
        if (vm.AlbumArtFile != null && vm.AlbumArtFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await vm.AlbumArtFile.CopyToAsync(ms);
            albumArt = ms.ToArray();
        }

        await _albumService.UpdateAlbumAsync(vm.Id, vm.Name, vm.ReleaseYear, albumArt, vm.RemoveAlbumArt);
        
        TempData["SuccessMessage"] = "Album updated successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _albumService.DeleteAlbumAsync(id);
        TempData["SuccessMessage"] = "Album deleted successfully!";
        return RedirectToAction("Index");
    }
}
