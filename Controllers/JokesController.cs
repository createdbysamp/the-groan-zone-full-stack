using System.Threading.Tasks;
using GroanZone.Models;
using GroanZone.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

namespace GroanZone.Controllers;

[Route("jokes")]
public class JokesController : Controller
{
    private readonly ApplicationContext _context;
    private const string SessionUserId = "userId";

    public JokesController(ApplicationContext context)
    {
        _context = context;
    }

    // general routing
    // ----- JOKES index action method ----- //
    [HttpGet("all")]
    public async Task<IActionResult> JokesIndex()
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // UNPROTECTED HOME PAGE
        // PROTECTION ... added :(
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // get the current user ID for RatedByMe variable

        // Get jokes from database
        var jokes = await _context
            .Jokes.AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Ratings) // added ratings
            // .ThenInclude(r => r.User) // added user for each rating
            .ToListAsync();

        // map joke to NEW JokeViewModel
        var jokeViewModels = jokes
            .Select(joke => new JokeViewModel
            {
                Id = joke.Id,
                SetUp = joke.SetUp,
                PunchLine = joke.PunchLine,
                CreatedAt = joke.CreatedAt,
                UpdatedAt = joke.UpdatedAt,
                AuthorUsername = joke.User?.UserName ?? "Unknown",
                RatingCount = joke.Ratings.Count, // count ratings for this joke
                RatedByMe = joke.Ratings.Any(r => r.UserId == uid), // true if userId has rated
                AvgRating = joke.Ratings.Any()
                    ? Math.Round(joke.Ratings.Average(r => r.RatingValue), 1)
                    : 0,
                UserId = joke.User.Id,
            })
            .ToList();

        var vm = new JokesIndexViewModel { AllJokes = jokeViewModels };
        return View(vm);
    }

    // ----- NEW JOKES ACTION ----- //
    [HttpGet("new")]
    public IActionResult NewJokeForm()
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // VIEW
        return View(new JokeViewModel());
    }

    // --- NEW JOKE POST --- //
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewJoke(JokeViewModel viewModel)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        if (!ModelState.IsValid)
        {
            return View("NewJokeForm", viewModel);
        }

        var joke = new Joke
        {
            SetUp = viewModel.SetUp,
            PunchLine = viewModel.PunchLine,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = (int)userId,
        };

        _context.Jokes.Add(joke);
        await _context.SaveChangesAsync();
        return RedirectToAction("JokeDetails", new { id = joke.Id });
    }

    // --- INDIVIDUAL JOKE VIEW --- //
    [HttpGet("{id}")]
    public async Task<IActionResult> JokeDetails(int id)
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var joke = await _context
            .Jokes.AsNoTracking()
            .Include(m => m.User) // include user
            .Include(m => m.Ratings) // include ratings
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        // if not found, return 404 error
        if (joke is null)
            return NotFound();

        // skipp mapping the entity to a viewModel and instead return view of joke
        return View(joke);
    }

    // ---- EDIT FORM GET ---- //
    [HttpGet("{id}/edit")]
    public async Task<IActionResult> EditJokeForm(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        var joke = await _context.Jokes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (joke is null)
            return NotFound();

        // MAKE SURE USER IS WHO CREATED IT
        if (joke.UserId != currentUserId)
        {
            return Forbid();
        }

        var vm = new JokeViewModel
        {
            Id = joke.Id,
            SetUp = joke.SetUp,
            PunchLine = joke.PunchLine,
        };
        return View(vm);
    }

    // --- EDIT FORM POST --- //
    [HttpPost()]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateJoke(int id, JokeViewModel viewModel)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("EditJokeForm", viewModel);
        }

        var joke = await _context.Jokes.FindAsync(id);
        if (joke == null)
        {
            return NotFound();
        }
        // MAKE SURE USER IS WHO CREATED IT
        if (joke.UserId != currentUserId)
        {
            return Forbid();
        }

        // map the joke to the VIEWMODEL data

        joke.SetUp = viewModel.SetUp;
        joke.PunchLine = viewModel.PunchLine;
        joke.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction("JokeDetails", new { id = joke.Id });
    }

    // ---- GET DELETE CONFIRMATION ACTION ---- //
    [HttpGet("{id}/delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var joke = await _context.Jokes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (joke is null)
            return NotFound();

        // MUST BE USER-WHO-CREATED
        if (joke.UserId != currentUserId)
        {
            return Forbid();
        }

        // MAP TO NEW VIEWMODEL OF JOKE
        var vm = new JokeViewModel
        {
            Id = joke.Id,
            SetUp = joke.SetUp,
            PunchLine = joke.PunchLine,
        };
        // RETURN VIEW
        return View("ConfirmDelete", vm);
    }

    // --- POST DELETE ACTION --- //
    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJoke(int id, JokeViewModel vm)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // IF JOKE DOES NOT EXIST
        if (vm.Id is null || vm.Id.Value != id)
        {
            return BadRequest();
        }
        var joke = await _context.Jokes.FindAsync(id);
        if (joke is null)
        {
            return NotFound();
        }

        // UPDATE DB
        _context.Jokes.Remove(joke);
        await _context.SaveChangesAsync();

        // REDIRECT TO HOME (REQUIRED BY WIREFRAME)
        return RedirectToAction("JokesIndex");
    }

    // --- HTTP POST ACTION FOR RATING --- //
    [HttpPost("{id}/rate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, int ratingValue)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // Check if the user has already liked this post
        var alreadyRated = await _context.Ratings.FirstOrDefaultAsync(r =>
            r.UserId == uid && r.JokeId == id
        );
        // if it's rated ...
        if (alreadyRated != null)
        {
            // set new ratingValue
            alreadyRated.RatingValue = ratingValue;
        }
        else
        {
            // set it to new class of Rating
            var newRate = new Rating
            {
                // map variables to variables
                UserId = uid,
                JokeId = id,
                RatingValue = ratingValue,
            };
            // add to ratings table
            _context.Ratings.Add(newRate);
        }

        // save changes
        await _context.SaveChangesAsync();

        // return to jokeDetails page
        return RedirectToAction("JokeDetails", new { id });
    }
}

// IF YOU'RE READING THIS, YOU'RE DOING GREAT! :)
