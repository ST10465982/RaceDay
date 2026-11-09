namespace RaceDay.Web.Models;

public class MessageDto { public string Message { get; set; } = ""; public int EventID { get; set; } public int CategoryID { get; set; } public int EnrollmentID { get; set; } public int ResultID { get; set; } }
public class UserDto { public int UserID { get; set; } public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; public string Email { get; set; } = ""; public string Role { get; set; } = ""; }
public class EventDto
{
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public int MaximumParticipants { get; set; }
    public string Status { get; set; } = "";
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = "";
    public List<CategoryDto> Categories { get; set; } = new();
    public string? BannerUrl { get; set; }
}
public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryType { get; set; } = "";
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public decimal? DistanceKm { get; set; }
    public string? Description { get; set; }
}
public class EnrollmentDto
{
    public int EnrollmentID { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = "";
    public string ParticipantEmail { get; set; } = "";
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime EnrollmentDate { get; set; }
    public string EnrollmentStatus { get; set; } = "";
}
public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrollmentID { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = "";
    public string Location { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public TimeSpan FinishTime { get; set; }
    public int OverallPosition { get; set; }
    public int CategoryPosition { get; set; }
    public int TotalFinishers { get; set; }
    public DateTime RecordedAt { get; set; }
}
public class ProfileDto
{
    public int ProfileID { get; set; }
    public int UserID { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
public class ImageUrlDto { public string? Url { get; set; } }
