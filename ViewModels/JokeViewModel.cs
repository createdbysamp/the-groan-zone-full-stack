using System.ComponentModel.DataAnnotations;

namespace GroanZone.ViewModels;

public class JokeViewModel
{
    // null on create; populated on edit

    public int? Id { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Set-up must be at least 2 char")]
    public string SetUp { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "Punchline must be at least 2 char")]
    public string PunchLine { get; set; } = string.Empty;

    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime UpdatedAt = DateTime.UtcNow;

    // tie in for user - public authorization
    public string AuthorUsername { get; set; } = string.Empty;

    // tie in for ratings
    public int RatingCount { get; set; }
    public bool RatedByMe { get; set; }
    public double AvgRating { get; set; }

    // tie in for userId
    public int UserId { get; set; }
}
