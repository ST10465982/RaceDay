using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class ProfileRequest
    {
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactNumber { get; set; }
    }
}