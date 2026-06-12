using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;
using page.Models.ViewModels;
using page.Services;

namespace page.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUpload;
        private readonly IPassportService _passportService;
        private readonly INotificationService _notificationService;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileUploadService fileUpload,
            IPassportService passportService,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _fileUpload = fileUpload;
            _passportService = passportService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var passport = await _passportService.GetPassportAsync(user.Id);

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                TotalReviews = await _context.Reviews.CountAsync(r => r.UserId == user.Id),
                TotalStories = await _context.TravelStories.CountAsync(s => s.UserId == user.Id),
                TotalSaved = await _context.SavedDestinations.CountAsync(s => s.UserId == user.Id),
                TotalVisited = await _context.VisitedDestinations.CountAsync(v => v.UserId == user.Id),
                Stamps = passport.Stamps,
                HasExplorerBadge = passport.HasExplorerBadge,
                RecentStories = await _context.TravelStories
                    .Include(s => s.Destination)
                    .Where(s => s.UserId == user.Id)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                SavedDestinations = await _context.SavedDestinations
                    .Include(s => s.Destination)
                    .Where(s => s.UserId == user.Id)
                    .OrderByDescending(s => s.SavedAt)
                    .Take(5)
                    .ToListAsync()
            };

            ViewBag.Notifications = await _notificationService.GetRecentAsync(user.Id);
            ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(user.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, IFormFile? avatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            user.FullName = fullName ?? user.FullName;

            if (avatarFile != null)
            {
                var url = await _fileUpload.UploadAsync(avatarFile, "uploads/avatars");
                if (url != null)
                {
                    if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl.StartsWith("/uploads/"))
                        _fileUpload.Delete(user.AvatarUrl);
                    user.AvatarUrl = url;
                }
            }

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            await _notificationService.MarkReadAsync(id);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            await _notificationService.MarkAllReadAsync(user.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}
