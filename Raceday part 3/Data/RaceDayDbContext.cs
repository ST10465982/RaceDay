using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<RaceEvent> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /*==========================================================
              USERS
            ==========================================================*/

            modelBuilder.Entity<User>()
                .ToTable("Users");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"
                ));


            /*==========================================================
              USER PROFILES
            ==========================================================*/

            modelBuilder.Entity<UserProfile>()
                .ToTable("UserProfiles");

            modelBuilder.Entity<UserProfile>()
                .HasIndex(p => p.UserID)
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);


            /*==========================================================
              EVENTS
            ==========================================================*/

            modelBuilder.Entity<RaceEvent>()
                .ToTable("Events");

            modelBuilder.Entity<RaceEvent>()
                .Property(e => e.DistanceKm)
                .HasPrecision(6, 2);

            modelBuilder.Entity<RaceEvent>()
                .Property(e => e.Status)
                .HasDefaultValue("Open");

            modelBuilder.Entity<RaceEvent>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<RaceEvent>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RaceEvent>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Events_EventType",
                        "[EventType] IN ('Run', 'Walk', 'Cycle')"
                    );

                    t.HasCheckConstraint(
                        "CK_Events_Status",
                        "[Status] IN ('Open', 'Closed', 'Completed', 'Cancelled')"
                    );

                    t.HasCheckConstraint(
                        "CK_Events_DistanceKm",
                        "[DistanceKm] > 0"
                    );

                    t.HasCheckConstraint(
                        "CK_Events_MaximumParticipants",
                        "[MaximumParticipants] > 0"
                    );
                });


            /*==========================================================
              CATEGORIES
            ==========================================================*/

            modelBuilder.Entity<Category>()
                .ToTable("Categories");

            modelBuilder.Entity<Category>()
                .Property(c => c.DistanceKm)
                .HasPrecision(6, 2);

            // Category names must be unique within the same event.
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.EventID, c.CategoryName })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasOne(c => c.RaceEvent)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Categories_CategoryType",
                        "[CategoryType] IN ('Age', 'Distance')"
                    );

                    t.HasCheckConstraint(
                        "CK_Categories_MinimumAge",
                        "[MinimumAge] IS NULL OR [MinimumAge] >= 0"
                    );

                    t.HasCheckConstraint(
                        "CK_Categories_MaximumAge",
                        "[MaximumAge] IS NULL OR [MaximumAge] >= 0"
                    );

                    t.HasCheckConstraint(
                        "CK_Categories_AgeRange",
                        "[MinimumAge] IS NULL OR [MaximumAge] IS NULL OR [MaximumAge] >= [MinimumAge]"
                    );

                    t.HasCheckConstraint(
                        "CK_Categories_DistanceKm",
                        "[DistanceKm] IS NULL OR [DistanceKm] > 0"
                    );
                });


            /*==========================================================
              ENROLLMENTS
            ==========================================================*/

            modelBuilder.Entity<Enrollment>()
                .ToTable("Enrollments");

            modelBuilder.Entity<Enrollment>()
                .Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<Enrollment>()
                .Property(e => e.EnrollmentStatus)
                .HasDefaultValue("Active");

            // Prevent the same participant from entering the same event twice.
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new
                {
                    e.EventID,
                    e.ParticipantID
                })
                .IsUnique();

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.RaceEvent)
                .WithMany(re => re.Enrollments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Participant)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Enrollments_Status",
                    "[EnrollmentStatus] IN ('Active', 'Completed', 'Cancelled')"
                ));


            /*==========================================================
              RESULTS
            ==========================================================*/

            modelBuilder.Entity<Result>()
                .ToTable("Results");

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrollmentID)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .Property(r => r.RecordedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrollment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrollmentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Results_OverallPosition",
                        "[OverallPosition] > 0"
                    );

                    t.HasCheckConstraint(
                        "CK_Results_CategoryPosition",
                        "[CategoryPosition] > 0"
                    );
                });
        }
    }
}