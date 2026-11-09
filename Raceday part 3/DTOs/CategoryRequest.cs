using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string CategoryType { get; set; } = string.Empty;

        [Range(0, 120)]
        public int? MinimumAge { get; set; }

        [Range(0, 120)]
        public int? MaximumAge { get; set; }

        [Range(0.1, 1000)]
        public decimal? DistanceKm { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }
    }
}