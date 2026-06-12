using Microsoft.AspNetCore.Identity;

namespace page.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<SavedDestination> SavedDestinations { get; set; } = new List<SavedDestination>();
        public ICollection<VisitedDestination> VisitedDestinations { get; set; } = new List<VisitedDestination>();
        public ICollection<UserRegionStamp> RegionStamps { get; set; } = new List<UserRegionStamp>();
        public ICollection<TravelStory> TravelStories { get; set; } = new List<TravelStory>();

    }
}
