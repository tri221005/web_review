using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using page.Data;
using page.Models;
using page.Services;

namespace page.Controllers
{
    public class StoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IToastNotification _toast;
        private readonly IFileUploadService _fileUpload;
        private readonly INotificationService _notificationService;
        private readonly IProfanityFilterService _profanityFilter;

        public StoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IToastNotification toast, IFileUploadService fileUpload, INotificationService notificationService, IProfanityFilterService profanityFilter)
        {
            _context = context;
            _userManager = userManager;
            _toast = toast;
            _fileUpload = fileUpload;
            _notificationService = notificationService;
            _profanityFilter = profanityFilter;
        }

        public async Task<IActionResult> Index()
        {
            var stories = await _context.TravelStories
                .Include(s => s.User)
                .Include(s => s.Destination)
                .Include(s => s.Comments).ThenInclude(c => c.User)
                .OrderByDescending(s => s.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(stories);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int? destinationId)
        {
            ViewBag.Destinations = await _context.Destinations.OrderBy(d => d.Name).ToListAsync();
            return View(new TravelStory { DestinationId = destinationId ?? 0 });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TravelStory story, IFormFile? imageFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (story.DestinationId <= 0 || string.IsNullOrWhiteSpace(story.Title) || string.IsNullOrWhiteSpace(story.Content))
            {
                ViewBag.Destinations = await _context.Destinations.OrderBy(d => d.Name).ToListAsync();
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin.");
                return View(story);
            }

            if (imageFile != null)
            {
                var url = await _fileUpload.UploadAsync(imageFile, "uploads/stories");
                if (url != null) story.ImageUrl = url;
            }

            story.UserId = user.Id;
            story.CreatedAt = DateTime.UtcNow;
            _context.TravelStories.Add(story);
            await _context.SaveChangesAsync();

            _toast.AddSuccessToastMessage("Câu chuyện của bạn đã được đăng!");
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upvote(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existing = await _context.StoryUpvotes.FindAsync(user.Id, id);
            if (existing != null)
            {
                _toast.AddInfoToastMessage("Bạn đã upvote câu chuyện này rồi.");
                return RedirectToAction(nameof(Index));
            }

            var story = await _context.TravelStories.FindAsync(id);
            if (story == null) return NotFound();

            _context.StoryUpvotes.Add(new StoryUpvote { UserId = user.Id, StoryId = id });
            story.UpvoteCount++;
            await _context.SaveChangesAsync();

            if (story.UserId != user.Id)
                await _notificationService.SendAsync(story.UserId, "Upvote mới", $"{user.UserName} đã thích câu chuyện của bạn", "/Stories");

            _toast.AddSuccessToastMessage("Cảm ơn bạn đã upvote!");
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int storyId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(content))
            {
                _toast.AddWarningToastMessage("Vui lòng nhập nội dung bình luận.");
                return RedirectToAction(nameof(Index));
            }

            var filterResult = _profanityFilter.Validate(content);
            if (!filterResult.IsClean)
            {
                _toast.AddErrorToastMessage("⚠️ Bình luận chứa từ ngữ không phù hợp.");
                return RedirectToAction(nameof(Index));
            }

            var story = await _context.TravelStories.FindAsync(storyId);
            if (story == null) return NotFound();

            _context.StoryComments.Add(new StoryComment
            {
                StoryId = storyId,
                UserId = user.Id,
                Content = content.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (story.UserId != user.Id)
                await _notificationService.SendAsync(story.UserId, "Bình luận mới", $"{user.UserName} đã bình luận về câu chuyện của bạn", "/Stories");

            _toast.AddSuccessToastMessage("Đã thêm bình luận!");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStory(int id)
        {
            var story = await _context.TravelStories.FindAsync(id);
            if (story == null) return NotFound();

            _context.TravelStories.Remove(story);
            await _context.SaveChangesAsync();
            
            _toast.AddSuccessToastMessage("Đã xóa câu chuyện thành công.");
            return RedirectToAction(nameof(Index));
        }
    }
}
