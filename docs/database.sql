-- Create Database
CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO

-- Event Table
CREATE TABLE Event (
    EventID INT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(100) NOT NULL
);

-- Sponsor Table
CREATE TABLE Sponsor (
    SponsorID INT PRIMARY KEY IDENTITY(1,1),
    SponsorName NVARCHAR(100) NOT NULL,
    ContactInfo NVARCHAR(100)
);

-- Category Table
CREATE TABLE Category (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    AgeRange NVARCHAR(50) NOT NULL,
    SponsorID INT NOT NULL,
    FOREIGN KEY (SponsorID) REFERENCES Sponsor(SponsorID)
);

-- Participant Table
CREATE TABLE Participant (
    ParticipantID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Age INT CHECK (Age >= 16)
);

-- Enrollment Table
CREATE TABLE Enrollment (
    EnrollmentID INT PRIMARY KEY IDENTITY(1,1),
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
    RaceID INT PRIMARY KEY IDENTITY(1,1),
    CategoryID INT NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

-- Result Table
CREATE TABLE Result (
    ResultID INT PRIMARY KEY IDENTITY(1,1),
    EnrollmentID INT NOT NULL,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,
    FOREIGN KEY (EnrollmentID) REFERENCES Enrollment(EnrollmentID)
);

-- Junction Table for Event-Sponsor (M:N Relationship)
CREATE TABLE EventSponsor (
    EventID INT NOT NULL,
    SponsorID INT NOT NULL,
    PRIMARY KEY (EventID, SponsorID),
    FOREIGN KEY (EventID) REFERENCES Event(EventID),
    FOREIGN KEY (SponsorID) REFERENCES Sponsor(SponsorID)
);

-- Sample Data Inserts
-- Sponsors
INSERT INTO Sponsor (SponsorName, ContactInfo) VALUES
('Nike SA', 'nike@events.com'),
('Adidas Africa', 'adidas@events.com');

-- Events
INSERT INTO Event (EventName, EventDate, Location) VALUES
('Durban Marathon', '2026-09-15', 'Durban'),
('Cape Town Cycle Tour', '2026-10-01', 'Cape Town'),
('Johannesburg Sprint', '2026-11-05', 'Johannesburg');

-- Categories
INSERT INTO Category (CategoryName, Distance, AgeRange, SponsorID) VALUES
('Full Marathon', 42.2, '18-40', 1),
('Half Marathon', 21.1, '18-50', 2),
('Road Cycling', 100.0, '16-60', 1),
('100m Sprint', 0.1, '16-30', 2);

-- Participants
INSERT INTO Participant (FirstName, LastName, Email, Age) VALUES
('Tankiso', 'Mokoena', 'tankiso@example.com', 22),
('Lerato', 'Pillay', 'lerato@example.com', 25);

-- Enrollments
INSERT INTO Enrollment (ParticipantID, CategoryID, EventID, Status) VALUES
(1, 1, 1, 'Registered'),
(2, 2, 1, 'Registered'),
(1, 3, 2, 'Registered'),
(2, 4, 3, 'Registered');

-- Races
INSERT INTO Race (CategoryID, Distance) VALUES
(1, 42.2),
(2, 21.1),
(3, 100.0),
(4, 0.1);

-- Results
INSERT INTO Result (EnrollmentID, FinishTime, Position) VALUES
(1, '03:45:00', 1),
(2, '04:10:00', 2),
(3, '05:30:00', 1),
(4, '00:12:00', 1);

-- Event-Sponsor Links
INSERT INTO EventSponsor (EventID, SponsorID) VALUES
(1, 1),
(2, 2),
(3, 1);
