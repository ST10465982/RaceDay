using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class ResultRequest
    {
        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OverallPosition { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryPosition { get; set; }
    }
}