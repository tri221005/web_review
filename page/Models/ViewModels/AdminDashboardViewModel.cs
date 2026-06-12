namespace page.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalDestinations { get; set; }
        public int TotalUsers { get; set; }
        public int TotalReviews { get; set; }
        public int TotalStories { get; set; }
        public int TotalItineraries { get; set; }
        public double AverageRating { get; set; }
        public List<Destination> TopDestinations { get; set; } = new();
        public List<Destination> PendingDestinations { get; set; } = new();
        public List<UserSummary> RecentUsers { get; set; } = new();
    }

    public class UserSummary
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public int ReviewCount { get; set; }
        public int StoryCount { get; set; }
    }
}
