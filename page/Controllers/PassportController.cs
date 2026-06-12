using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using page.Models;
using page.Services;

namespace page.Controllers
{
    [Authorize]
    public class PassportController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPassportService _passportService;

        public PassportController(UserManager<ApplicationUser> userManager, IPassportService passportService)
        {
            _userManager = userManager;
            _passportService = passportService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = await _passportService.GetPassportAsync(user.Id);
            return View(model);
        }
    }
}
