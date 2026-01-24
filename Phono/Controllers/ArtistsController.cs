using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Phono.Models;
using Phono.ViewModels;
using Phono.Services;

namespace Phono.Controllers;

[Authorize]
public class ArtistsController : Controller
{
    private readonly ArtistService _artistService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ArtistsController(ArtistService artistService, UserManager<ApplicationUser> userManager)
    {
        _artistService = artistService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var artists = await _artistService.GetAllArtistsAsync();
        return View(artists);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EditArtistViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EditArtistViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _artistService.CreateArtistAsync(vm.Name, vm.Bio);
            TempData["SuccessMessage"] = "Artist created successfully!";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Name", ex.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var artist = await _artistService.GetArtistByIdAsync(id);
        if (artist == null)
            return NotFound();

        var vm = new EditArtistViewModel
        {
            Id = artist.Id,
            Name = artist.Name,
            Bio = artist.Bio,
            TrackCount = artist.TrackArtists.Count
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditArtistViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _artistService.UpdateArtistAsync(vm.Id, vm.Name, vm.Bio);
            TempData["SuccessMessage"] = "Artist updated successfully!";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Name", ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _artistService.DeleteArtistAsync(id);
        TempData["SuccessMessage"] = "Artist deleted successfully!";
        return RedirectToAction("Index");
    }
}
