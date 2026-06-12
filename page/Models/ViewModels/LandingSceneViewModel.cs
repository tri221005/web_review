namespace page.Models.ViewModels
{
    public class LandingSceneViewModel
    {
        public string SceneKey { get; set; } = string.Empty;
        public int? DestinationId { get; set; }
        public string DestinationName { get; set; } = string.Empty;
    }

    public class HomeIndexViewModel
    {
        public Dictionary<string, LandingSceneViewModel> SceneLinks { get; set; } = new();
        public List<TravelStory> RecentStories { get; set; } = new();
    }
}
