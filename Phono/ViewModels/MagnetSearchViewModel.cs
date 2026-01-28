using Phono.Models;

namespace Phono.ViewModels;

public class MagnetSearchViewModel
{
    public string? Query { get; set; }
    public MagnetApiSearchResult? Result { get; set; }
}
