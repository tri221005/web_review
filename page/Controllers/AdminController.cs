using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;
using page.Models.ViewModels;

namespace page.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalDestinations = await _context.Destinations.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalReviews = await _context.Reviews.CountAsync();
            var totalStories = await _context.TravelStories.CountAsync();

            var avgRating = totalReviews > 0
                ? await _context.Reviews.AverageAsync(r => (double)r.Rating)
                : 0;

            var topDestinations = await _context.Destinations
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.ReviewCount)
                .Take(5)
                .ToListAsync();

            var users = await _context.Users.ToListAsync();
            var recentUsers = new List<UserSummary>();
            foreach (var u in users.OrderByDescending(u => u.LockoutEnd ?? DateTimeOffset.MaxValue).Take(10))
            {
                recentUsers.Add(new UserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    ReviewCount = await _context.Reviews.CountAsync(r => r.UserId == u.Id),
                    StoryCount = await _context.TravelStories.CountAsync(s => s.UserId == u.Id)
                });
            }

            var pendingDestinations = await _context.Destinations
                .Where(d => !d.IsApproved)
                .Include(d => d.SubmittedByUser)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            return View(new AdminDashboardViewModel
            {
                TotalDestinations = totalDestinations,
                TotalUsers = totalUsers,
                TotalReviews = totalReviews,
                TotalStories = totalStories,

                AverageRating = avgRating,
                TopDestinations = topDestinations,
                PendingDestinations = pendingDestinations,
                RecentUsers = recentUsers
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDestination(int id)
        {
            var dest = await _context.Destinations.FindAsync(id);
            if (dest == null) return NotFound();

            dest.IsApproved = true;
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Đã duyệt điểm đến: {dest.Name}";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin.";
                return RedirectToAction(nameof(Dashboard));
            }
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Đã xóa người dùng.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
