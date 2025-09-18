using System.ComponentModel.DataAnnotations;

namespace GroanZone.ViewModels;

public class ProfileViewModel
{
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public int JokesAdded { get; set; }
    public int JokesRated { get; set; }
}
