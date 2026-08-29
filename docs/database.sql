-- RaceDay Database Schema
CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO

-- Organiser Table
CREATE TABLE Organiser (
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL
);

-- Participant Table
CREATE TABLE Participant (
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL
);

-- Event Table
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Date DATE NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    Description NVARCHAR(255),
    OrganiserID INT NOT NULL,
    FOREIGN KEY (OrganiserID) REFERENCES Organiser(OrganiserID)
);

-- Category Table
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventID INT NOT NULL,
    FOREIGN KEY (EventID) REFERENCES Event(EventID)
);

-- Enrolment Table
CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolDate DATE DEFAULT GETDATE(),
    FOREIGN KEY (ParticipantID) REFERENCES Participant(ParticipantID),
    FOREIGN KEY (EventID) REFERENCES Event(EventID),
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

-- Result Table
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    Time TIME NOT NULL,
    Position INT NOT NULL,
    FOREIGN KEY (ParticipantID) REFERENCES Participant(ParticipantID),
    FOREIGN KEY (EventID) REFERENCES Event(EventID)
);

-- Seed Data
INSERT INTO Organiser (Name, Email, Password)
VALUES ('John Mokoena', 'john@raceday.co.za', 'hashedpassword1'),
       ('Lerato Khumalo', 'lerato@raceday.co.za', 'hashedpassword2');

INSERT INTO Participant (Name, Email, Password)
VALUES ('Thabo Dlamini', 'thabo@raceday.co.za', 'hashedpassword3'),
       ('Nomsa Ndlovu', 'nomsa@raceday.co.za', 'hashedpassword4');

INSERT INTO Event (Name, Date, Location, Description, OrganiserID)
VALUES ('Durban Marathon', '2026-09-15', 'Durban Beachfront', 'Annual coastal marathon', 1),
       ('Jozi Cycle Challenge', '2026-10-10', 'Johannesburg CBD', 'Urban cycling event', 2),
       ('Cape Town Fun Run', '2026-11-05', 'Green Point Stadium', 'Community fun run', 1);

INSERT INTO Category (Name, Distance, EventID)
VALUES ('5km Run', 5.00, 3),
       ('10km Run', 10.00, 3),
       ('42km Marathon', 42.00, 1),
       ('20km Cycle', 20.00, 2),
       ('50km Cycle', 50.00, 2);

INSERT INTO Enrolment (ParticipantID, EventID, CategoryID)
VALUES (1, 1, 3),
       (2, 3, 1),
       (1, 2, 4);

INSERT INTO Result (ParticipantID, EventID, Time, Position)
VALUES (1, 1, '02:45:00', 5),
       (2, 3, '00:30:00', 2),
       (1, 2, '01:10:00', 3);
