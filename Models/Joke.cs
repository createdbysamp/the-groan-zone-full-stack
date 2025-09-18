using System.ComponentModel.DataAnnotations;

namespace GroanZone.Models;

public class Joke
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Set-up must be at least 2 char")]
    public string SetUp { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "Punchline must be at least 2 char")]
    public string PunchLine { get; set; } = string.Empty;

    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime UpdatedAt = DateTime.UtcNow;

    // --- ONE TO MANY RELATIONSHIP PROPERTIES --- //

    // FOREIGN KEY FOR THE USER WHO CREATED JOKE
    // navigation property for the user
    public int UserId { get; set; }
    public User? User { get; set; }

    // many-to-many relationship with user through the rating join table
    public List<Rating> Ratings { get; set; } = [];
}
