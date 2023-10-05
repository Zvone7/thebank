CREATE TABLE Borrower
(
    Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PersonId      UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Person](Id),
    YearlySalary  DECIMAL(18, 5) NOT NULL,
    CurrentEquity DECIMAL(18, 5) NOT NULL
);