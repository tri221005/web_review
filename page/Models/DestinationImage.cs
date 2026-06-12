using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace page.Models
{
    public class DestinationImage
    {
        public int Id { get; set; }

        [Required]
        public int DestinationId { get; set; }
        [ForeignKey("DestinationId")]
        public Destination? Destination { get; set; }

        [Required, MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Caption { get; set; }

        public int SortOrder { get; set; }
    }
}
