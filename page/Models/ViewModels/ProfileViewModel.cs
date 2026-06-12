namespace page.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public int TotalReviews { get; set; }
        public int TotalStories { get; set; }
        public int TotalSaved { get; set; }
        public int TotalVisited { get; set; }
        public List<RegionStampInfo> Stamps { get; set; } = new();
        public bool HasExplorerBadge { get; set; }
        public List<TravelStory> RecentStories { get; set; } = new();
        public List<SavedDestination> SavedDestinations { get; set; } = new();
    }
}
