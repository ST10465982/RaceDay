using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrollmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OverallPosition { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryPosition { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("EnrollmentID")]
        public Enrollment Enrollment { get; set; } = null!;
    }
}