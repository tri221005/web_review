using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;
using page.Services;

namespace page.Controllers
{
    public class AIController : Controller
    {
        private readonly IAIAssistantService _aiService;
        private readonly ApplicationDbContext _context;

        public AIController(IAIAssistantService aiService, ApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        // AI Chat Assistant
        [HttpGet]
        public async Task<IActionResult> Chat()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng AI Assistant" });
            }

            var response = await _aiService.ChatAsync(userId, request.Message, request.ContextType, request.RelatedDestinationId);
            return Json(new { success = true, response });
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var history = await _aiService.GetConversationHistoryAsync(userId);
            return Json(new { success = true, history });
        }

        // AI Itinerary Generator
        [HttpGet]
        public async Task<IActionResult> GenerateItinerary()
        {
            ViewBag.Destinations = await _context.Destinations.Where(d => d.IsApproved).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateItinerary([FromBody] ItineraryRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var itinerary = await _aiService.GenerateItineraryAsync(
                userId, 
                request.Prompt, 
                request.DurationDays, 
                request.StartDate, 
                request.Budget);

            if (itinerary != null)
            {
                return Json(new { success = true, itineraryId = itinerary.Id });
            }

            return Json(new { success = false, message = "Không thể tạo lộ trình. Vui lòng thử lại." });
        }

        [HttpGet]
        public async Task<IActionResult> ViewItinerary(int id)
        {
            var itinerary = await _context.Itineraries
                .Include(i => i.Days)
                    .ThenInclude(d => d.Activities)
                        .ThenInclude(a => a.Destination)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (itinerary == null)
                return NotFound();

            return View(itinerary);
        }

        [HttpGet]
        public async Task<IActionResult> MyItineraries()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var itineraries = await _context.Itineraries
                .Include(i => i.Days)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(itineraries);
        }

        // Personalized Recommendations
        [HttpGet]
        public async Task<IActionResult> Recommendations()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var recommendations = await _aiService.GetPersonalizedRecommendationsAsync(userId);
            ViewBag.SavedDestinationIds = await _context.SavedDestinations
                .Where(s => s.UserId == userId)
                .Select(s => s.DestinationId)
                .ToListAsync();

            return View(recommendations);
        }

        // Image Recognition (Future Feature)
        [HttpPost]
        public async Task<IActionResult> RecognizeLocation(IFormFile image)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (image == null || image.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn hình ảnh" });
            }

            // Upload and process image (placeholder)
            var imageUrl = "/uploads/recognized/" + Guid.NewGuid() + Path.GetExtension(image.FileName);
            
            var result = await _aiService.RecognizeLocationFromImageAsync(userId, imageUrl);
            
            return Json(new { 
                success = true, 
                message = "Chức năng nhận diện đang được phát triển",
                analysis = result?.AIAnalysis 
            });
        }

        // Review Verification
        [HttpPost]
        public async Task<IActionResult> VerifyReview(int reviewId, string proofData)
        {
            var result = await _aiService.VerifyReviewAsync(reviewId, proofData);
            
            if (result)
            {
                return Json(new { success = true, message = "Đánh giá đã được xác minh!" });
            }

            return Json(new { success = false, message = "Không thể xác minh đánh giá" });
        }

        // Voice Search (Future Feature - Frontend will use Web Speech API)
        [HttpGet]
        public IActionResult VoiceSearch()
        {
            return View();
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? ContextType { get; set; }
        public int? RelatedDestinationId { get; set; }
    }

    public class ItineraryRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public int DurationDays { get; set; } = 3;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public decimal Budget { get; set; } = 5000000;
    }
}
