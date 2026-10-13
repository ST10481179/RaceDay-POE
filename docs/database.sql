CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO

-- Participant Table
CREATE TABLE Participant (
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Age INT NOT NULL
);

-- Sponsor Table
CREATE TABLE Sponsor (
    SponsorID INT IDENTITY(1,1) PRIMARY KEY,
    SponsorName NVARCHAR(100) NOT NULL,
    ContactInfo NVARCHAR(150)
);

-- Event Table
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    SponsorID INT NULL,
    FOREIGN KEY (SponsorID) REFERENCES Sponsor(SponsorID)
);

-- Category Table
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    AgeRange NVARCHAR(50),
    EventID INT NOT NULL,
    FOREIGN KEY (EventID) REFERENCES Event(EventID)
);

-- Enrollment Table
CREATE TABLE Enrollment (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    EventID INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Registered',
    FOREIGN KEY (ParticipantID) REFERENCES Participant(ParticipantID),
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
    FOREIGN KEY (EventID) REFERENCES Event(EventID)
);

-- Race Table
CREATE TABLE Race (
    RaceID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

-- Result Table
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollment(EnrollmentID)
);

-- Seed Data
INSERT INTO Sponsor (SponsorName, ContactInfo)
VALUES ('Nike SA', 'nike@raceday.co.za'),
       ('Discovery Health', 'discovery@raceday.co.za');

INSERT INTO Participant (FirstName, LastName, Email, Age)
VALUES ('Thabo', 'Dlamini', 'thabo@raceday.co.za', 25),
       ('Nomsa', 'Ndlovu', 'nomsa@raceday.co.za', 30);

INSERT INTO Event (EventName, EventDate, Location, SponsorID)
VALUES ('Durban Marathon', '2026-09-15', 'Durban Beachfront', 1),
       ('Jozi Cycle Challenge', '2026-10-10', 'Johannesburg CBD', 2),
       ('Cape Town Fun Run', '2026-11-05', 'Green Point Stadium', 1);

INSERT INTO Category (CategoryName, Distance, AgeRange, EventID)
VALUES ('5km Run', 5.00, 'Under 20', 3),
       ('10km Run', 10.00, 'Senior', 3),
       ('42km Marathon', 42.00, 'Adult', 1),
       ('20km Cycle', 20.00, 'Adult', 2),
       ('50km Cycle', 50.00, 'Senior', 2);

INSERT INTO Enrollment (ParticipantID, CategoryID, EventID, Status)
VALUES (1, 3, 1, 'Registered'),
       (2, 1, 3, 'Registered'),
       (1, 4, 2, 'Registered');

INSERT INTO Race (CategoryID, Distance)
VALUES (3, 42.00),
       (4, 20.00),
       (1, 5.00);

INSERT INTO Result (EnrollmentID, FinishTime, Position)
VALUES (1, '02:45:00', 5),
       (2, '00:30:00', 2),
       (3, '01:10:00', 3);

