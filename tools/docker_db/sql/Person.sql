CREATE TABLE Person
(
    Id       UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(100) NOT NULL,
    Email    NVARCHAR(100) UNIQUE NOT NULL,
    RoleId   INT FOREIGN KEY REFERENCES Role (Id),
    Ssn      NVARCHAR(11) UNIQUE NOT NULL
);