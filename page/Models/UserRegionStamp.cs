using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace page.Models
{
    public class UserRegionStamp
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(50)]
        public string Region { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Source { get; set; } = string.Empty;

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }
}
