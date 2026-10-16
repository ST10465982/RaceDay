using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EnrollmentRequest
    {
        [Required]
        public int CategoryID { get; set; }
    }
}