/*==============================================================
    RaceDay Database
    PROG6212 - Programming 2B
    Part 1: System Planning and Database
==============================================================*/

-- Create the RaceDay database if it does not already exist
IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/*==============================================================
    Remove existing tables in dependency order.
    This allows the script to be tested more than once.
==============================================================*/

DROP TABLE IF EXISTS Results;
DROP TABLE IF EXISTS Enrollments;
DROP TABLE IF EXISTS Events;
DROP TABLE IF EXISTS UserProfiles;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Users;
GO


/*==============================================================
    1. USERS
==============================================================*/

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Users
        PRIMARY KEY (UserID),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO


/*==============================================================
    2. USER PROFILES
==============================================================*/

CREATE TABLE UserProfiles
(
    ProfileID INT IDENTITY(1,1) NOT NULL,
    UserID INT NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    DateOfBirth DATE NOT NULL,
    EmergencyContactName NVARCHAR(100) NOT NULL,
    EmergencyContactNumber NVARCHAR(20) NOT NULL,

    CONSTRAINT PK_UserProfiles
        PRIMARY KEY (ProfileID),

    CONSTRAINT UQ_UserProfiles_UserID
        UNIQUE (UserID),

    CONSTRAINT FK_UserProfiles_Users
        FOREIGN KEY (UserID)
        REFERENCES Users(UserID)
);
GO


/*==============================================================
    3. CATEGORIES
==============================================================*/

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryID),

    CONSTRAINT UQ_Categories_CategoryName
        UNIQUE (CategoryName)
);
GO


/*==============================================================
    4. EVENTS
==============================================================*/

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) NOT NULL,
    OrganiserID INT NOT NULL,
    CategoryID INT NOT NULL,
    EventName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATE NOT NULL,
    StartTime TIME(0) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    MaximumParticipants INT NOT NULL,
    Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Events_Status DEFAULT 'Open',
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Events_CreatedAt DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Events
        PRIMARY KEY (EventID),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Events_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),

    CONSTRAINT CK_Events_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Events_MaximumParticipants
        CHECK (MaximumParticipants > 0),

    CONSTRAINT CK_Events_Status
        CHECK
        (
            Status IN
            (
                'Planned',
                'Open',
                'Closed',
                'Completed',
                'Cancelled'
            )
        )
);
GO


/*==============================================================
    5. ENROLLMENTS
==============================================================*/

CREATE TABLE Enrollments
(
    EnrollmentID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrollmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrollments_EnrollmentDate DEFAULT SYSDATETIME(),
    EnrollmentStatus NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrollments_Status DEFAULT 'Active',

    CONSTRAINT PK_Enrollments
        PRIMARY KEY (EnrollmentID),

    CONSTRAINT FK_Enrollments_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT FK_Enrollments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Users(UserID),

    CONSTRAINT UQ_Enrollments_EventParticipant
        UNIQUE (EventID, ParticipantID),

    CONSTRAINT CK_Enrollments_Status
        CHECK
        (
            EnrollmentStatus IN
            (
                'Active',
                'Completed',
                'Cancelled'
            )
        )
);
GO


/*==============================================================
    6. RESULTS
==============================================================*/

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) NOT NULL,
    EnrollmentID INT NOT NULL,
    FinishTime TIME(0) NOT NULL,
    OverallPosition INT NOT NULL,
    CategoryPosition INT NOT NULL,
    RecordedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Results_RecordedAt DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultID),

    CONSTRAINT FK_Results_Enrollments
        FOREIGN KEY (EnrollmentID)
        REFERENCES Enrollments(EnrollmentID),

    CONSTRAINT UQ_Results_EnrollmentID
        UNIQUE (EnrollmentID),

    CONSTRAINT CK_Results_OverallPosition
        CHECK (OverallPosition > 0),

    CONSTRAINT CK_Results_CategoryPosition
        CHECK (CategoryPosition > 0)
);
GO


/*==============================================================
    SEED DATA
==============================================================*/

-- Two Organisers and two Participants
INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, Role)
VALUES
    ('Thando', 'Mokoena',
     'thando.mokoena@raceday.co.za',
     'HASHED_PASSWORD_ORGANISER_1',
     'Organiser'),

    ('Naledi', 'Jacobs',
     'naledi.jacobs@raceday.co.za',
     'HASHED_PASSWORD_ORGANISER_2',
     'Organiser'),

    ('Kagiso', 'Dlamini',
     'kagiso.dlamini@email.co.za',
     'HASHED_PASSWORD_PARTICIPANT_1',
     'Participant'),

    ('Lerato', 'Naidoo',
     'lerato.naidoo@email.co.za',
     'HASHED_PASSWORD_PARTICIPANT_2',
     'Participant');
GO


-- User profiles
INSERT INTO UserProfiles
    (
        UserID,
        PhoneNumber,
        DateOfBirth,
        EmergencyContactName,
        EmergencyContactNumber
    )
VALUES
    (1, '0825550101', '1990-05-12',
     'Sipho Mokoena', '0825550201'),

    (2, '0835550102', '1992-09-23',
     'Ayesha Jacobs', '0835550202'),

    (3, '0715550103', '2001-03-18',
     'Nomsa Dlamini', '0715550203'),

    (4, '0725550104', '2000-11-06',
     'Ravi Naidoo', '0725550204');
GO


-- Event categories
INSERT INTO Categories
    (CategoryName, Description)
VALUES
    ('Road Running',
     'Timed road-running events for recreational and competitive runners.'),

    ('Community Walking',
     'Community walking events promoting health and social participation.'),

    ('Road Cycling',
     'Organised cycling events held on approved road routes.');
GO


-- Three RaceDay events
INSERT INTO Events
    (
        OrganiserID,
        CategoryID,
        EventName,
        Description,
        EventDate,
        StartTime,
        Location,
        DistanceKm,
        MaximumParticipants,
        Status
    )
VALUES
    (
        1,
        1,
        'Soweto Heritage 10K',
        'A community 10 kilometre road race through Soweto.',
        '2026-08-15',
        '07:00:00',
        'Soweto, Johannesburg',
        10.00,
        500,
        'Completed'
    ),

    (
        2,
        2,
        'Cape Town Spring Community Walk',
        'A social walking event along the Cape Town promenade.',
        '2026-09-20',
        '08:00:00',
        'Sea Point, Cape Town',
        5.00,
        300,
        'Open'
    ),

    (
        1,
        3,
        'Durban Coast Charity Cycle',
        'A charity cycling event along the Durban coastline.',
        '2026-10-03',
        '06:30:00',
        'Durban, KwaZulu-Natal',
        40.00,
        400,
        'Open'
    );
GO


-- Sample event enrollments
INSERT INTO Enrollments
    (
        EventID,
        ParticipantID,
        EnrollmentDate,
        EnrollmentStatus
    )
VALUES
    (
        1,
        3,
        '2026-07-20T10:15:00',
        'Completed'
    ),

    (
        1,
        4,
        '2026-07-22T14:30:00',
        'Completed'
    ),

    (
        2,
        3,
        '2026-08-25T09:20:00',
        'Active'
    ),

    (
        3,
        4,
        '2026-08-26T11:45:00',
        'Active'
    );
GO


-- Sample results for the completed event
INSERT INTO Results
    (
        EnrollmentID,
        FinishTime,
        OverallPosition,
        CategoryPosition,
        RecordedAt
    )
VALUES
    (
        1,
        '00:48:32',
        18,
        12,
        '2026-08-15T10:30:00'
    ),

    (
        2,
        '00:53:14',
        31,
        20,
        '2026-08-15T10:35:00'
    );
GO


/*==============================================================
    VERIFICATION QUERIES
==============================================================*/

SELECT * FROM Users;
SELECT * FROM UserProfiles;
SELECT * FROM Categories;
SELECT * FROM Events;
SELECT * FROM Enrollments;
SELECT * FROM Results;
GO