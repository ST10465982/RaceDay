using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string EnrollmentStatus { get; set; } = "Active";

        [ForeignKey("EventID")]
        public RaceEvent RaceEvent { get; set; } = null!;

        [ForeignKey("ParticipantID")]
        public User Participant { get; set; } = null!;

        [ForeignKey("CategoryID")]
        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}