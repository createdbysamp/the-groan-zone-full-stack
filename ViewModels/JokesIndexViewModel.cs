using GroanZone.Models;

namespace GroanZone.ViewModels;

public class JokesIndexViewModel
{
    public List<JokeViewModel> AllJokes { get; set; } = [];
    public int TotalCount => AllJokes.Count;
}
