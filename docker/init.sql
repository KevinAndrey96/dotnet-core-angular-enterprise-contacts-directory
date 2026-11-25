-- ==============================================================================
-- Contacts Directory Enterprise Database Schema (SQL Server)
-- Platform: Hubbec Contacts Directory
-- Author: Kevin Andrey Herrera Silva <kahs_kevin@hotmail.com>
-- ==============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DirectoryDB')
BEGIN
    CREATE DATABASE DirectoryDB;
END
GO

USE DirectoryDB;
GO

-- 1. Table: Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        firstName NVARCHAR(100) NOT NULL,
        lastName NVARCHAR(100) NOT NULL,
        documentType NVARCHAR(10) NOT NULL DEFAULT 'CC',
        documentNumber NVARCHAR(20) NOT NULL,
        phone NVARCHAR(20) NOT NULL,
        email NVARCHAR(150) NOT NULL,
        password NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_email_phone_documentNumber 
    ON Users (email, phone, documentNumber);

    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_email 
    ON Users (email);
END
GO

-- 2. Table: Contacts
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contacts')
BEGIN
    CREATE TABLE Contacts (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdUser BIGINT NOT NULL,
        IdContact BIGINT NULL,
        firstName NVARCHAR(100) NOT NULL,
        lastName NVARCHAR(100) NOT NULL,
        documentType NVARCHAR(10) NOT NULL DEFAULT 'CC',
        documentNumber NVARCHAR(20) NOT NULL,
        phone NVARCHAR(20) NOT NULL,
        email NVARCHAR(150) NOT NULL,
        relationship NVARCHAR(50) NOT NULL DEFAULT 'Amigo',
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    CREATE NONCLUSTERED INDEX IX_Contacts_IdUser 
    ON Contacts (IdUser);

    CREATE NONCLUSTERED INDEX IX_Contacts_IdContact 
    ON Contacts (IdContact);
END
GO

-- 3. Initial Seed Data
IF NOT EXISTS (SELECT 1 FROM Users WHERE email = 'kaherrera@hubbec.com')
BEGIN
    INSERT INTO Users (firstName, lastName, documentType, documentNumber, phone, email, password, CreatedAt)
    VALUES 
    ('Kevin Andrey', 'Herrera Silva', 'CC', '1032485121', '3185563353', 'kaherrera@hubbec.com', 'Password123!', SYSUTCDATETIME()),
    ('Diana', 'Prince', 'CC', '1098765432', '3101234567', 'diana.prince@hubbec.com', 'Password123!', SYSUTCDATETIME()),
    ('Bruce', 'Wayne', 'CC', '1087654321', '3209876543', 'bruce.wayne@hubbec.com', 'Password123!', SYSUTCDATETIME()),
    ('Clark', 'Kent', 'CC', '1076543210', '3154567890', 'clark.kent@hubbec.com', 'Password123!', SYSUTCDATETIME());

    DECLARE @Uid1 BIGINT = (SELECT Id FROM Users WHERE email = 'kaherrera@hubbec.com');
    DECLARE @Uid2 BIGINT = (SELECT Id FROM Users WHERE email = 'diana.prince@hubbec.com');
    DECLARE @Uid3 BIGINT = (SELECT Id FROM Users WHERE email = 'bruce.wayne@hubbec.com');
    DECLARE @Uid4 BIGINT = (SELECT Id FROM Users WHERE email = 'clark.kent@hubbec.com');

    -- Seed associations for mutual contacts calculation:
    -- Kevin has Diana and Bruce
    -- Diana has Kevin, Bruce, and Clark
    -- Mutual contact between Kevin and Diana: Bruce!
    INSERT INTO Contacts (IdUser, IdContact, firstName, lastName, documentType, documentNumber, phone, email, relationship, CreatedAt)
    VALUES
    (@Uid1, @Uid2, 'Diana', 'Prince', 'CC', '1098765432', '3101234567', 'diana.prince@hubbec.com', 'Colega', SYSUTCDATETIME()),
    (@Uid1, @Uid3, 'Bruce', 'Wayne', 'CC', '1087654321', '3209876543', 'bruce.wayne@hubbec.com', 'Amigo', SYSUTCDATETIME()),
    (@Uid2, @Uid1, 'Kevin Andrey', 'Herrera Silva', 'CC', '1032485121', '3185563353', 'kaherrera@hubbec.com', 'Colega', SYSUTCDATETIME()),
    (@Uid2, @Uid3, 'Bruce', 'Wayne', 'CC', '1087654321', '3209876543', 'bruce.wayne@hubbec.com', 'Amigo', SYSUTCDATETIME()),
    (@Uid2, @Uid4, 'Clark', 'Kent', 'CC', '1076543210', '3154567890', 'clark.kent@hubbec.com', 'Otro', SYSUTCDATETIME());
END
GO
