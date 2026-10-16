using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EventRequest
    {
        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000)]
        public decimal DistanceKm { get; set; }

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int MaximumParticipants { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open";
    }
}