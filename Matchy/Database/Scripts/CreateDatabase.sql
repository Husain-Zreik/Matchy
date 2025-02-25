CREATE DATABASE MemoryGameDB;

USE MemoryGameDB;

-- Users Table
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    Age INT NOT NULL,
    CreatedAt DATETIME NOT NULL
);

-- Scores Table
CREATE TABLE Scores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Score INT NOT NULL,
    Level INT NOT NULL,
    AchievedAt DATETIME DEFAULT GETDATE()
);

-- Settings Table
CREATE TABLE Settings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    SettingName NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(255) NOT NULL
);


-- Rate Table
CREATE TABLE Rate (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    RatedAt DATETIME DEFAULT GETDATE()
);

USE MemoryGameDB; 

-- Insert the settings into the Settings table
INSERT INTO Settings (SettingName, SettingValue)
VALUES ('MaxLevel', '3'),  
       ('TimeBeforeReveal', '3000'); 