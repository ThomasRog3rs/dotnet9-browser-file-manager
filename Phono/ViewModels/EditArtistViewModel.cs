using System.ComponentModel.DataAnnotations;

namespace Phono.ViewModels;

public class EditArtistViewModel
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    [Display(Name = "Artist Name")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    [Display(Name = "Biography")]
    public string? Bio { get; set; }
    
    public int TrackCount { get; set; }
}
