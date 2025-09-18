using Microsoft.EntityFrameworkCore;

namespace GroanZone.Models;

public class ApplicationContext : DbContext
{
    // first migrated, but Db for Album model
    public DbSet<Joke> Jokes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Rating> Ratings { get; set; }

    // add DbSet for our like model
    // public DbSet<Rating> Ratings { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options) { }
}
