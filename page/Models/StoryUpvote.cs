using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace page.Models
{
    public class StoryUpvote
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int StoryId { get; set; }
        [ForeignKey("StoryId")]
        public TravelStory? Story { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
