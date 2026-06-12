using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models.ViewModels;

namespace page.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var destinations = await _context.Destinations
                .Where(d => d.LandingSceneKey != null)
                .ToListAsync();

            var sceneLinks = destinations.ToDictionary(
                d => d.LandingSceneKey!,
                d => new LandingSceneViewModel
                {
                    SceneKey = d.LandingSceneKey!,
                    DestinationId = d.Id,
                    DestinationName = d.Name
                });

            var model = new HomeIndexViewModel
            {
                SceneLinks = sceneLinks,
                RecentStories = await _context.TravelStories
                    .Include(s => s.User)
                    .Include(s => s.Destination)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(3)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
