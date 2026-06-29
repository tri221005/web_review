using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using page.Data;
using page.Models;

namespace page.Services
{
    public interface IAIAssistantService
    {
        Task<string> ChatAsync(string userId, string message, string? contextType = null, int? relatedDestinationId = null);
        Task<List<AIConversation>> GetConversationHistoryAsync(string userId, int count = 20);
        Task<Itinerary?> GenerateItineraryAsync(string userId, string prompt, int durationDays, DateTime startDate, decimal budget);
        Task<List<Destination>> GetPersonalizedRecommendationsAsync(string userId, int count = 10);
        Task<RecognizedLocation?> RecognizeLocationFromImageAsync(string userId, string imageUrl);
        Task TrackUserPreferenceAsync(string userId, string preferenceType, string preferenceValue);
        Task<bool> VerifyReviewAsync(int reviewId, string proofData);
    }

    public class AIAssistantService : IAIAssistantService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _aiApiKey;
        private readonly string _aiApiUrl;

        public AIAssistantService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _aiApiKey = configuration["AI:ApiKey"] ?? "";
            _aiApiUrl = configuration["AI:ApiUrl"] ?? "http://localhost:11434/v1/chat/completions";
        }

        public async Task<string> ChatAsync(string userId, string message, string? contextType = null, int? relatedDestinationId = null)
        {
            // Save user message
            var conversation = new AIConversation
            {
                UserId = userId,
                Message = message,
                ContextType = contextType,
                RelatedDestinationId = relatedDestinationId,
                CreatedAt = DateTime.UtcNow
            };

            // Get conversation history for context (last 10 messages)
            var history = await _context.AIConversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Build context-aware prompt
            var systemPrompt = @"Bạn là một trợ lý AI chuyên gia về du lịch Việt Nam. 
            Nhiệm vụ của bạn:
            - Tư vấn điểm đến, lộ trình, kinh nghiệm du lịch
            - Gợi ý ẩm thực, văn hóa địa phương
            - Hỗ trợ lập kế hoạch chuyến đi
            - Trả lời bằng tiếng Việt tự nhiên, thân thiện
            - Nếu câu hỏi không liên quan du lịch, nhẹ nhàng hướng người dùng về chủ đề du lịch";

            var historyContext = new StringBuilder();
            foreach (var msg in history.Reverse())
            {
                historyContext.AppendLine($"User: {msg.Message}");
                historyContext.AppendLine($"AI: {msg.AIResponse}");
            }

            var userPrompt = $"{historyContext}\nUser: {message}\nAI:";

            try
            {
                var payload = new
                {
                    model = "hf.co/nguyenviet/PhoGPT-4B-Chat-GGUF:Q4_K_M",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                _httpClient.DefaultRequestHeaders.Clear();
                if (!string.IsNullOrEmpty(_aiApiKey))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_aiApiKey}");

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_aiApiUrl, content);

                string aiResponse;
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = JsonDocument.Parse(responseString);
                    aiResponse = jsonDocument.RootElement.GetProperty("choices")[0]
                        .GetProperty("message").GetProperty("content").GetString() ?? "Xin lỗi, tôi chưa hiểu câu hỏi.";
                }
                else
                {
                    aiResponse = "Xin lỗi, hệ thống AI đang bận. Vui lòng thử lại sau.";
                }
            }
            catch (Exception ex)
            {
                aiResponse = $"Xin lỗi, có lỗi xảy ra: {ex.Message}";
            }

            conversation.AIResponse = aiResponse;
            _context.AIConversations.Add(conversation);

            // Track preferences from user message
            await TrackPreferencesFromMessageAsync(userId, message);

            await _context.SaveChangesAsync();
            return aiResponse;
        }

        public async Task<List<AIConversation>> GetConversationHistoryAsync(string userId, int count = 20)
        {
            return await _context.AIConversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Itinerary?> GenerateItineraryAsync(string userId, string prompt, int durationDays, DateTime startDate, decimal budget)
        {
            var systemPrompt = @"Bạn là một chuyên gia lập kế hoạch du lịch. 
            Hãy tạo một lộ trình chi tiết với định dạng JSON như sau:
            {
                ""title"": ""Tên chuyến đi"",
                ""description"": ""Mô tả ngắn"",
                ""days"": [
                    {
                        ""dayNumber"": 1,
                        ""title"": ""Ngày 1: ..."",
                        ""activities"": [
                            {
                                ""startTime"": ""08:00"",
                                ""endTime"": ""10:00"",
                                ""title"": ""Hoạt động"",
                                ""description"": ""Mô tả"",
                                ""activityType"": ""Visit"",
                                ""estimatedCost"": 100000
                            }
                        ]
                    }
                ]
            }
            Activity types: Visit, Eat, Rest, Transport";

            try
            {
                var payload = new
                {
                    model = "hf.co/nguyenviet/PhoGPT-4B-Chat-GGUF:Q4_K_M",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = $"Tạo lộ trình {durationDays} ngày từ '{prompt}' với ngân sách {budget:N0} VNĐ, bắt đầu {startDate:dd/MM/yyyy}" }
                    },
                    temperature = 0.8,
                    max_tokens = 2000
                };

                _httpClient.DefaultRequestHeaders.Clear();
                if (!string.IsNullOrEmpty(_aiApiKey))
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_aiApiKey}");

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_aiApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var jsonDocument = JsonDocument.Parse(responseString);
                    var aiContent = jsonDocument.RootElement.GetProperty("choices")[0]
                        .GetProperty("message").GetProperty("content").GetString() ?? "";

                    // Extract JSON from response
                    var jsonStart = aiContent.IndexOf('{');
                    var jsonEnd = aiContent.LastIndexOf('}');
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonStr = aiContent.Substring(jsonStart, jsonEnd - jsonStart + 1);
                        var itineraryData = JsonSerializer.Deserialize<ItineraryData>(jsonStr);

                        if (itineraryData != null)
                        {
                            var itinerary = new Itinerary
                            {
                                UserId = userId,
                                Title = itineraryData.Title,
                                Description = itineraryData.Description,
                                DurationDays = durationDays,
                                EstimatedBudget = budget,
                                AIPrompt = prompt,
                                IsAIGenerated = true,
                                StartDate = startDate,
                                EndDate = startDate.AddDays(durationDays - 1),
                                Days = new List<ItineraryDay>()
                            };

                            foreach (var dayData in itineraryData.Days)
                            {
                                var day = new ItineraryDay
                                {
                                    DayNumber = dayData.DayNumber,
                                    Title = dayData.Title,
                                    Description = "",
                                    Activities = new List<ItineraryActivity>()
                                };

                                foreach (var activityData in dayData.Activities)
                                {
                                    day.Activities.Add(new ItineraryActivity
                                    {
                                        StartTime = ParseTime(activityData.StartTime),
                                        EndTime = ParseTime(activityData.EndTime),
                                        Title = activityData.Title,
                                        Description = activityData.Description,
                                        ActivityType = activityData.ActivityType,
                                        EstimatedCost = activityData.EstimatedCost
                                    });
                                }

                                itinerary.Days.Add(day);
                            }

                            _context.Itineraries.Add(itinerary);
                            await _context.SaveChangesAsync();
                            return itinerary;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating itinerary: {ex.Message}");
            }

            return null;
        }

        public async Task<List<Destination>> GetPersonalizedRecommendationsAsync(string userId, int count = 10)
        {
            // Get user preferences
            var preferences = await _context.UserPreferences
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Score)
                .Take(5)
                .ToListAsync();

            if (!preferences.Any())
            {
                // Return popular destinations for new users
                return await _context.Destinations
                    .Where(d => d.IsApproved)
                    .OrderByDescending(d => d.AverageRating)
                    .ThenByDescending(d => d.ReviewCount)
                    .Take(count)
                    .ToListAsync();
            }

            // Build recommendation query based on preferences
            var query = _context.Destinations.Where(d => d.IsApproved).AsQueryable();

            var locationPrefs = preferences.Where(p => p.PreferenceType == "location").Select(p => p.PreferenceValue).ToList();
            var typePrefs = preferences.Where(p => p.PreferenceType == "type").Select(p => p.PreferenceValue).ToList();

            if (locationPrefs.Any())
            {
                query = query.Where(d => locationPrefs.Contains(d.Location));
            }

            if (typePrefs.Any())
            {
                query = query.Where(d => typePrefs.Contains(d.Type));
            }

            return await query
                .OrderByDescending(d => d.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<RecognizedLocation?> RecognizeLocationFromImageAsync(string userId, string imageUrl)
        {
            // This would integrate with an image recognition API (e.g., Google Vision, Azure Computer Vision)
            // For now, we'll create a placeholder entry
            var recognizedLocation = new RecognizedLocation
            {
                UserId = userId,
                ImageUrl = imageUrl,
                ConfidenceScore = 0.0f,
                AIAnalysis = "Chức năng nhận diện hình ảnh sẽ được tích hợp trong tương lai.",
                UploadedAt = DateTime.UtcNow
            };

            _context.RecognizedLocations.Add(recognizedLocation);
            await _context.SaveChangesAsync();

            return recognizedLocation;
        }

        public async Task TrackUserPreferenceAsync(string userId, string preferenceType, string preferenceValue)
        {
            var existing = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.PreferenceType == preferenceType && 
                                         p.PreferenceValue == preferenceValue);

            if (existing != null)
            {
                existing.Score++;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _context.UserPreferences.Add(new UserPreference
                {
                    UserId = userId,
                    PreferenceType = preferenceType,
                    PreferenceValue = preferenceValue,
                    Score = 1,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> VerifyReviewAsync(int reviewId, string proofData)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            // Generate a simple hash for verification (blockchain-inspired)
            var hash = GenerateHash($"{reviewId}-{review.UserId}-{review.CreatedAt}-{proofData}");

            var verification = new ReviewVerification
            {
                ReviewId = reviewId,
                IsVerified = true,
                VerificationHash = hash,
                VerificationProof = proofData,
                VerifiedAt = DateTime.UtcNow,
                VerifiedBy = "system"
            };

            _context.ReviewVerifications.Add(verification);
            await _context.SaveChangesAsync();

            return true;
        }

        #region Private Helpers

        private async Task TrackPreferencesFromMessageAsync(string userId, string message)
        {
            // Simple keyword-based preference extraction
            var locations = new[] { "miền bắc", "miền trung", "miền nam", "hà nội", "sài gòn", "đà nẵng", "huế", "nha trang" };
            var types = new[] { "biển", "núi", "lịch sử", "văn hóa", "ẩm thực", "nghỉ dưỡng" };

            var lowerMessage = message.ToLower();

            foreach (var location in locations)
            {
                if (lowerMessage.Contains(location))
                {
                    await TrackUserPreferenceAsync(userId, "location", location);
                    break;
                }
            }

            foreach (var type in types)
            {
                if (lowerMessage.Contains(type))
                {
                    await TrackUserPreferenceAsync(userId, "type", type);
                    break;
                }
            }
        }

        private TimeSpan ParseTime(string timeStr)
        {
            if (TimeSpan.TryParse(timeStr, out var result))
                return result;

            // Try parsing HH:mm format
            if (timeStr.Contains(":"))
            {
                var parts = timeStr.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes))
                {
                    return new TimeSpan(hours, minutes, 0);
                }
            }

            return new TimeSpan(8, 0, 0); // Default 8:00 AM
        }

        private string GenerateHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        #endregion
    }

    // Helper classes for JSON deserialization
    public class ItineraryData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<DayData> Days { get; set; } = new();
    }

    public class DayData
    {
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<ActivityData> Activities { get; set; } = new();
    }

    public class ActivityData
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActivityType { get; set; } = "Visit";
        public decimal EstimatedCost { get; set; }
    }
}
