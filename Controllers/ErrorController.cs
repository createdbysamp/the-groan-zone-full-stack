using Microsoft.AspNetCore.Mvc;

namespace GroanZone.Controllers;

public class ErrorController : Controller
{
    [HttpGet("error/{code}")]
    public IActionResult Handle(int code)
    {
        if (code == 404)
        {
            Console.WriteLine("----------- if you're reading this, you're reached 404 --------//");
            // Serve a custom view for 404 errors
            return View("PageNotFound");
        }
        else if (code == 403)
            return View("Forbidden");
        else if (code == 400)
            return View("BadRequest");

        // Optional: handle other codes
        return View("ServerError");
    }

    [HttpGet("error/boom")]
    public IActionResult Boom()
    {
        // This is a test method that will intentionally throw a 500 error.
        // It's a useful way to test our custom error page without introducing a bug.
        return new StatusCodeResult(500);
    }
}
