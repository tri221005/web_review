using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace page.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        [Required, Range(1, 5)]
        public int Rating { get; set; } // 1 to 5 stars
        
        [MaxLength(2000)]
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int DestinationId { get; set; }
        [ForeignKey("DestinationId")]
        public Destination? Destination { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
