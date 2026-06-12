using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using page.Data;
using page.Models;
using page.Services;
using System.Text.Json;

namespace page.Controllers
{
    public class DestinationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPassportService _passportService;
        private readonly IToastNotification _toast;
        private readonly INotificationService _notificationService;

        public DestinationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IPassportService passportService,
            IToastNotification toast,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _passportService = passportService;
            _toast = toast;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(string searchString, string location, string type, decimal? maxCost, string sortOrder)
        {
            var destinations = _context.Destinations.Where(d => d.IsApproved).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                destinations = destinations.Where(d => d.Name.Contains(searchString) || d.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(location))
            {
                destinations = destinations.Where(d => d.Location == location);
            }

            if (!string.IsNullOrEmpty(type))
            {
                destinations = destinations.Where(d => d.Type == type);
            }

            if (maxCost.HasValue)
            {
                destinations = destinations.Where(d => d.EstimatedCost <= maxCost.Value);
            }

            destinations = sortOrder switch
            {
                "rating_desc" => destinations.OrderByDescending(d => d.AverageRating),
                "rating_asc" => destinations.OrderBy(d => d.AverageRating),
                "cost_desc" => destinations.OrderByDescending(d => d.EstimatedCost),
                "cost_asc" => destinations.OrderBy(d => d.EstimatedCost),
                "name_asc" => destinations.OrderBy(d => d.Name),
                "name_desc" => destinations.OrderByDescending(d => d.Name),
                _ => destinations.OrderByDescending(d => d.AverageRating).ThenBy(d => d.EstimatedCost)
            };

            ViewBag.Locations = await _context.Destinations.Select(d => d.Location).Distinct().ToListAsync();
            ViewBag.Types = await _context.Destinations.Select(d => d.Type).Distinct().ToListAsync();
            ViewBag.CurrentSort = sortOrder;

            var list = await destinations.ToListAsync();
            ViewBag.MapDestinations = list.Where(d => d.Latitude.HasValue && d.Longitude.HasValue).ToList();

            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var destination = await _context.Destinations
                .Include(d => d.Reviews).ThenInclude(r => r.User)
                .Include(d => d.Comments).ThenInclude(c => c.User)
                .Include(d => d.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (destination == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            ViewBag.IsSaved = userId != null && await _context.SavedDestinations
                .AnyAsync(s => s.UserId == userId && s.DestinationId == id);
            ViewBag.IsVisited = userId != null && await _context.VisitedDestinations
                .AnyAsync(v => v.UserId == userId && v.DestinationId == id);
            ViewBag.SentimentTags = ReviewSentimentService.AnalyzeComments(
                destination.Reviews.Select(r => r.Comment));
            ViewBag.HeritageTimeline = ParseHeritageTimeline(destination.HeritageTimelineJson);
            ViewBag.RecentStories = await _context.TravelStories
                .Include(s => s.User)
                .Where(s => s.DestinationId == id)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(destination);
        }

        [Authorize]
        public async Task<IActionResult> Saved()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var saved = await _context.SavedDestinations
                .Include(s => s.Destination)
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            ViewBag.TotalCost = saved.Sum(s => s.Destination?.EstimatedCost ?? 0);
            ViewBag.Checklist = BuildChecklist(saved.Select(s => s.Destination).Where(d => d != null)!);

            return View(saved);
        }

        public async Task<IActionResult> Compare(int[] ids)
        {
            if (ids == null || ids.Length < 2)
            {
                _toast.AddWarningToastMessage("Vui lòng chọn ít nhất 2 điểm đến để so sánh.");
                return RedirectToAction(nameof(Index));
            }

            var destinations = await _context.Destinations
                .Where(d => ids.Contains(d.Id))
                .Take(3)
                .ToListAsync();

            return View(destinations);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDestinationComment(int destinationId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(content))
            {
                var comment = new DestinationComment
                {
                    DestinationId = destinationId,
                    UserId = user.Id,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DestinationComments.Add(comment);
                await _context.SaveChangesAsync();
                _toast.AddSuccessToastMessage("Đã đăng bình luận!");
            }

            return RedirectToAction(nameof(Details), new { id = destinationId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDestinationComment(int commentId)
        {
            var comment = await _context.DestinationComments.FindAsync(commentId);
            if (comment == null) return NotFound();

            var destinationId = comment.DestinationId;
            _context.DestinationComments.Remove(comment);
            await _context.SaveChangesAsync();

            _toast.AddSuccessToastMessage("Đã xóa bình luận.");
            return RedirectToAction(nameof(Details), new { id = destinationId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int destinationId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null) return NotFound();

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.DestinationId == destinationId && r.UserId == user.Id);
            
            if (existingReview != null)
            {
                _toast.AddErrorToastMessage("Bạn đã đánh giá địa điểm này rồi!");
                return RedirectToAction(nameof(Details), new { id = destinationId });
            }

            var review = new Review
            {
                DestinationId = destinationId,
                UserId = user.Id,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            var allRatings = await _context.Reviews
                .Where(r => r.DestinationId == destinationId)
                .Select(r => r.Rating)
                .ToListAsync();

            destination.AverageRating = allRatings.Any() ? allRatings.Average() : 0;
            destination.ReviewCount = allRatings.Count;
            await _context.SaveChangesAsync();

            await _passportService.AwardStampAsync(user.Id, destination!, "Review");

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
                await _notificationService.SendAsync(admin.Id, "Đánh giá mới", $"{user.UserName} đã đánh giá {destination!.Name}", $"/Destinations/Details/{destinationId}");

            _toast.AddSuccessToastMessage("Cảm ơn bạn đã đánh giá!");

            return RedirectToAction(nameof(Details), new { id = destinationId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId, int destinationId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                var allRatings = await _context.Reviews
                    .Where(r => r.DestinationId == destinationId)
                    .Select(r => r.Rating)
                    .ToListAsync();

                var destination = await _context.Destinations.FindAsync(destinationId);
                if (destination != null)
                {
                    destination.AverageRating = allRatings.Any() ? allRatings.Average() : 0;
                    destination.ReviewCount = allRatings.Count;
                    await _context.SaveChangesAsync();
                }

                _toast.AddSuccessToastMessage("Đã xóa đánh giá.");
            }
            return RedirectToAction(nameof(Details), new { id = destinationId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int reviewId, int destinationId, string comment)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                review.Comment = comment;
                await _context.SaveChangesAsync();
                _toast.AddSuccessToastMessage("Đã sửa đánh giá.");
            }
            return RedirectToAction(nameof(Details), new { id = destinationId });
        }
        [HttpPost]
        public async Task<IActionResult> EnhanceReview([FromBody] EnhanceReviewRequest request)
        {
            var keywords = request.Keywords?.Trim() ?? "";
            if (string.IsNullOrEmpty(keywords))
            {
                return Json(new { success = false, message = "Vui lòng nhập từ khóa" });
            }

            try
            {
                using var httpClient = new HttpClient();
                
                // API Key bạn vừa cung cấp
                string apiKey = "68d8171afb4e4d2fafd9342cdb50e669.tRd1phSLm767ox5645kPoReh";
                
                // LƯU Ý QUAN TRỌNG: Hãy thay đổi URL dưới đây thành Endpoint thực tế của dịch vụ Ollama/AI bạn đang dùng
                // Đã chuyển sang URL mặc định của Ollama khi chạy Local (Hỗ trợ chuẩn OpenAI)
                string apiUrl = "http://localhost:11434/v1/chat/completions"; 
                
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var prompt = $"Bạn là một travel blogger chuyên nghiệp. Dựa vào các từ khóa sau: '{keywords}', hãy viết một đoạn review du lịch thật cảm xúc, thơ mộng, dài khoảng 3-4 câu bằng tiếng Việt. KHÔNG GIẢI THÍCH, KHÔNG CHÀO HỎI MỞ ĐẦU, CHỈ TRẢ VỀ DUY NHẤT ĐOẠN REVIEW.";

                var payload = new
                {
                    model = "gemma3:1b", // Cập nhật model thành gemma3:1b theo yêu cầu
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = System.Text.Json.JsonDocument.Parse(responseString);
                    var root = jsonDocument.RootElement;
                    string enhancedText = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                    
                    return Json(new { success = true, enhancedText = enhancedText?.Trim() });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, enhancedText = "Xin lỗi, đã có lỗi kết nối với AI (Vui lòng kiểm tra lại URL Endpoint). Chi tiết: " + response.StatusCode });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, enhancedText = "Xin lỗi, đã có lỗi hệ thống xảy ra: " + ex.Message });
            }
        }

        public class EnhanceReviewRequest
        {
            public string Keywords { get; set; }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDestination(int destinationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null) return NotFound();

            var existing = await _context.SavedDestinations.FindAsync(user.Id, destinationId);
            if (existing == null)
            {
                _context.SavedDestinations.Add(new SavedDestination { UserId = user.Id, DestinationId = destinationId });
                await _context.SaveChangesAsync();
                await _passportService.AwardStampAsync(user.Id, destination, "Saved");
                _toast.AddSuccessToastMessage("Đã lưu vào yêu thích!");
            }

            return RedirectToAction(nameof(Details), new { id = destinationId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsaveDestination(int destinationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existing = await _context.SavedDestinations.FindAsync(user.Id, destinationId);
            if (existing != null)
            {
                _context.SavedDestinations.Remove(existing);
                await _context.SaveChangesAsync();
                _toast.AddInfoToastMessage("Đã bỏ lưu khỏi yêu thích.");
            }

            var returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Saved));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkVisited(int destinationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null) return NotFound();

            var existing = await _context.VisitedDestinations.FindAsync(user.Id, destinationId);
            if (existing == null)
            {
                _context.VisitedDestinations.Add(new VisitedDestination { UserId = user.Id, DestinationId = destinationId });
                await _context.SaveChangesAsync();
                await _passportService.AwardStampAsync(user.Id, destination, "Visited");
                _toast.AddSuccessToastMessage("Đã đánh dấu đã đến! Con dấu vùng miền đã được cập nhật.");
            }

            return RedirectToAction(nameof(Details), new { id = destinationId });
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Location,Type,EstimatedCost,ImageUrl,ImageFile,Latitude,Longitude,LandingSceneKey,HeritageTimelineJson")] Destination destination, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
                destination.IsApproved = isAdmin;
                if (user != null) {
                    destination.SubmittedByUserId = user.Id;
                }

                if (destination.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(env.WebRootPath, "images", "destinations");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + destination.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await destination.ImageFile.CopyToAsync(fileStream);
                    }
                    destination.ImageUrl = "/images/destinations/" + uniqueFileName;
                }

                _context.Add(destination);
                await _context.SaveChangesAsync();
                
                if (isAdmin) {
                    _toast.AddSuccessToastMessage("Đã thêm điểm đến thành công.");
                } else {
                    _toast.AddSuccessToastMessage("Đã gửi yêu cầu thêm điểm đến. Vui lòng chờ Admin duyệt!");
                }
                
                return RedirectToAction(nameof(Index));
            }
            return View(destination);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null) return NotFound();
            return View(destination);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,Type,EstimatedCost,ImageUrl,ImageFile,Latitude,Longitude,LandingSceneKey,HeritageTimelineJson")] Destination destination, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (id != destination.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (destination.ImageFile != null)
                    {
                        string uploadsFolder = Path.Combine(env.WebRootPath, "images", "destinations");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + destination.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await destination.ImageFile.CopyToAsync(fileStream);
                        }
                        destination.ImageUrl = "/images/destinations/" + uniqueFileName;
                    }

                    _context.Update(destination);
                    await _context.SaveChangesAsync();
                    _toast.AddSuccessToastMessage("Đã cập nhật điểm đến.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Destinations.AnyAsync(d => d.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(destination);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var destination = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id);
            if (destination == null) return NotFound();
            return View(destination);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination != null)
            {
                _context.Destinations.Remove(destination);
                await _context.SaveChangesAsync();
                _toast.AddSuccessToastMessage("Đã xóa điểm đến.");
            }
            return RedirectToAction(nameof(Index));
        }


        private static List<HeritageTimelineEntry> ParseHeritageTimeline(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<HeritageTimelineEntry>();
            try
            {
                return JsonSerializer.Deserialize<List<HeritageTimelineEntry>>(json) ?? new List<HeritageTimelineEntry>();
            }
            catch
            {
                return new List<HeritageTimelineEntry>();
            }
        }

        private static List<string> BuildChecklist(IEnumerable<Destination> destinations)
        {
            var items = new HashSet<string> { "Giấy tờ tùy thân", "Tiền mặt / thẻ ngân hàng", "Điện thoại & sạc" };
            var types = destinations.Select(d => d.Type).Distinct().ToList();

            if (types.Any(t => t.Contains("Thiên nhiên") || t.Contains("Nghỉ dưỡng")))
            {
                items.Add("Kem chống nắng");
                items.Add("Đồ bơi");
            }
            if (types.Any(t => t.Contains("Thiên nhiên")))
            {
                items.Add("Giày trekking");
                items.Add("Thuốc chống côn trùng");
            }
            if (types.Any(t => t.Contains("Văn hóa") || t.Contains("Di tích")))
            {
                items.Add("Trang phục lịch sự");
                items.Add("Máy ảnh");
            }

            return items.ToList();
        }
    }

    public class HeritageTimelineEntry
    {
        public string Year { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
