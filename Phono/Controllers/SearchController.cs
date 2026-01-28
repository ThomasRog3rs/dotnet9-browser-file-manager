using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phono.Services;
using Phono.ViewModels;

namespace Phono.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly MagnetApiService _magnetApiService;

    public SearchController(MagnetApiService magnetApiService)
    {
        _magnetApiService = magnetApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? query)
    {
        var vm = new MagnetSearchViewModel
        {
            Query = query
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            vm.Result = await _magnetApiService.SearchPirateBayAudioAsync(query);
        }

        return View(vm);
    }
}
