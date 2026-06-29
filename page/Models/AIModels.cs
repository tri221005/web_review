using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace page.Models
{
    // AI Conversation History
    public class AIConversation
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        
        public string Message { get; set; } = string.Empty;
        public string AIResponse { get; set; } = string.Empty;
        
        public string? ContextType { get; set; } // "destination", "itinerary", "general"
        public int? RelatedDestinationId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // AI-Generated Itinerary
    public class Itinerary
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public int DurationDays { get; set; }
        public decimal EstimatedBudget { get; set; }
        public string DifficultyLevel { get; set; } = "Medium"; // Easy, Medium, Hard
        
        public string? AIPrompt { get; set; }
        public bool IsAIGenerated { get; set; } = true;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<ItineraryDay> Days { get; set; } = new List<ItineraryDay>();
        public ICollection<ItineraryFavorite> FavoritedByUsers { get; set; } = new List<ItineraryFavorite>();
    }

    public class ItineraryDay
    {
        public int Id { get; set; }
        
        public int ItineraryId { get; set; }
        [ForeignKey("ItineraryId")]
        public Itinerary? Itinerary { get; set; }
        
        public int DayNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public ICollection<ItineraryActivity> Activities { get; set; } = new List<ItineraryActivity>();
    }

    public class ItineraryActivity
    {
        public int Id { get; set; }
        
        public int ItineraryDayId { get; set; }
        [ForeignKey("ItineraryDayId")]
        public ItineraryDay? Day { get; set; }
        
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActivityType { get; set; } = "Visit"; // Visit, Eat, Rest, Transport
        
        public int? DestinationId { get; set; }
        public Destination? Destination { get; set; }
        
        public decimal EstimatedCost { get; set; }
        public string? Notes { get; set; }
    }

    // Smart Recommendation Engine
    public class UserPreference
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public string PreferenceType { get; set; } = string.Empty; // "location", "type", "budget", "activity"
        public string PreferenceValue { get; set; } = string.Empty;
        public int Score { get; set; } = 1;
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        [Index(nameof(UserId), nameof(PreferenceType), nameof(PreferenceValue), IsUnique = true)]
    }

    // Image Recognition Results
    public class RecognizedLocation
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public string ImageUrl { get; set; } = string.Empty;
        public int? RecognizedDestinationId { get; set; }
        public Destination? RecognizedDestination { get; set; }
        
        public float ConfidenceScore { get; set; }
        public string AIAnalysis { get; set; } = string.Empty;
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    // Itinerary Favorites
    public class ItineraryFavorite
    {
        public int ItineraryId { get; set; }
        public Itinerary? Itinerary { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        
        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
    }

    // Review Verification (Blockchain-inspired)
    public class ReviewVerification
    {
        public int Id { get; set; }
        
        public int ReviewId { get; set; }
        public Review? Review { get; set; }
        
        public bool IsVerified { get; set; } = false;
        public string? VerificationHash { get; set; }
        public string? VerificationProof { get; set; } // GPS check-in, ticket photo, etc.
        
        public DateTime VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; } // "system", "admin", "ai"
    }

    // Voice Command Logs
    public class VoiceCommand
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public string CommandText { get; set; } = string.Empty;
        public string CommandType { get; set; } = string.Empty; // "search", "navigate", "filter"
        public string? Result { get; set; }
        
        public bool WasSuccessful { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }
}
