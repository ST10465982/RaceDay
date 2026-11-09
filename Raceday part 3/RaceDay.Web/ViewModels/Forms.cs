using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Models;

namespace RaceDay.Web.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
public class RegisterViewModel
{
    [Required, StringLength(50)] public string FirstName { get; set; } = "";
    [Required, StringLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
    [Required, RegularExpression("^(Organiser|Participant)$")] public string Role { get; set; } = "Participant";
}
public class EventFormViewModel
{
    public int EventID { get; set; }
    [Required, MaxLength(150)] public string EventName { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(14);
    [Required] public TimeSpan StartTime { get; set; } = new(7,0,0);
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    [Range(0.1,1000)] public decimal DistanceKm { get; set; } = 10;
    [Required, RegularExpression("^(Run|Walk|Cycle)$")] public string EventType { get; set; } = "Run";
    [Range(1,int.MaxValue)] public int MaximumParticipants { get; set; } = 200;
    [Required] public string Status { get; set; } = "Open";
    public IFormFile? Banner { get; set; }
    public string? ExistingBanner { get; set; }
}
public class CategoryFormViewModel
{
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    [Required, MaxLength(100)] public string CategoryName { get; set; } = "";
    [Required, RegularExpression("^(Age|Distance)$")] public string CategoryType { get; set; } = "Distance";
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public decimal? DistanceKm { get; set; }
    [MaxLength(300)] public string? Description { get; set; }
}
public class ResultFormViewModel
{
    public int EventID { get; set; }
    public int EnrollmentID { get; set; }
    public int ResultID { get; set; }
    [Required] public TimeSpan FinishTime { get; set; } = new(1,0,0);
    [Range(1,int.MaxValue)] public int OverallPosition { get; set; } = 1;
    [Range(1,int.MaxValue)] public int CategoryPosition { get; set; } = 1;
}
public class ProfileFormViewModel
{
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
    [MaxLength(100)] public string? EmergencyContactName { get; set; }
    [MaxLength(20)] public string? EmergencyContactNumber { get; set; }
    public string? ExistingPhotoUrl { get; set; }
    public IFormFile? Photo { get; set; }
}
public class OrganiserDashboardViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public Dictionary<int, int> EnrolmentCounts { get; set; } = new();
    public int TotalEnrolments => EnrolmentCounts.Values.Sum();
}
public class EventListViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public string? Search { get; set; }
    public string? Type { get; set; }
}
public class EventDetailsViewModel { public EventDto Event { get; set; } = new(); }
public class ResultsPageViewModel { public int EventID { get; set; } public string EventName { get; set; } = ""; public List<ResultDto> Results { get; set; } = new(); public List<EnrollmentDto> Enrollments { get; set; } = new(); }
