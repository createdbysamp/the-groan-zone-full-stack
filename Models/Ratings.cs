using System.ComponentModel.DataAnnotations;

namespace GroanZone.Models;

public class Rating
{
    [Key]
    public int Id { get; set; }

    // foreign key for user who liked post
    public int UserId { get; set; }

    // foreign key for the movie that was liked
    public int JokeId { get; set; }

    // navigation property for user
    public User? User { get; set; }

    // navigation property for album
    public Joke? Joke { get; set; }

    [Range(1, 4, ErrorMessage = "Groan must be between 1 & 4.")]
    public int RatingValue { get; set; }
}
