using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace page.Models
{
    public class Destination
    {
        public int Id { get; set; }
        
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty; // Khu vực (Miền Bắc, Miền Trung, Miền Nam, etc.)
        
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // Loại hình (Thiên nhiên, Lịch sử, Văn hóa, etc.)
        
        public decimal EstimatedCost { get; set; } // Chi phí dự kiến
        
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(50)]
        public string? LandingSceneKey { get; set; }

        public string? HeritageTimelineJson { get; set; }
        
        // Fields for Meshy 3D API integration
        public string? MeshyTaskId { get; set; }
        public string? Model3DUrl { get; set; } // URL to .glb file
        
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        
        public bool IsApproved { get; set; } = true;
        
        public string? SubmittedByUserId { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<SavedDestination> SavedByUsers { get; set; } = new List<SavedDestination>();
        public ICollection<TravelStory> Stories { get; set; } = new List<TravelStory>();
        public ICollection<DestinationImage> Images { get; set; } = new List<DestinationImage>();
        public ICollection<DestinationComment> Comments { get; set; } = new List<DestinationComment>();
    }
}
