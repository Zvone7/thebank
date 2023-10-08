using System.Data;
using System.Data.SqlClient;
using Dapper;
using Portal.Core.Providers;
using Portal.Models;

namespace Portal.Dbl.Providers;

public class LoanProvider : ILoanProvider
{
    private readonly string _connectionString_;

    public LoanProvider(string connectionString)
    {
        _connectionString_ = connectionString;
    }

    public async Task<IEnumerable<LoanWithPersons>> GetAllLoansWithPersonsAsync()
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        var loans = await dbConnection.QueryAsync<LoanDto>("SELECT * FROM Loan");

        var loansWithPersons = new List<LoanWithPersons>();
        foreach (var loan in loans)
        {
            var personIds = await dbConnection.QueryAsync<Guid>(
                "SELECT PersonId FROM PersonToLoan WHERE LoanId = @LoanId",
                new { LoanId = loan.Id }
            );
            var persons = new List<PersonDto>();
            foreach (var personId in personIds)
            {
                var person = await dbConnection.QueryFirstOrDefaultAsync<PersonDto>(
                    "SELECT * FROM Person WHERE Id = @Id",
                    new { Id = personId }
                );
                if (person != null)
                {
                    persons.Add(person);
                }
            }
            loansWithPersons.Add(new LoanWithPersons { LoanDto = loan, Persons = persons });
        }

        return loansWithPersons;
    }

    public async Task<LoanWithPersons> GetLoanWithPersonsByIdAsync(Guid loanId)
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        var loan = await dbConnection.QueryFirstOrDefaultAsync<LoanDto>("SELECT * FROM Loan where Id = @LoanId", new { LoanId = loanId });

        var personIds = await dbConnection.QueryAsync<Guid>(
            "SELECT PersonId FROM PersonToLoan WHERE LoanId = @LoanId",
            new { LoanId = loanId }
        );
        var persons = new List<PersonDto>();
        foreach (var personId in personIds)
        {
            var person = await dbConnection.QueryFirstOrDefaultAsync<PersonDto>(
                "SELECT * FROM Person WHERE Id = @Id",
                new { Id = personId }
            );
            if (person != null)
            {
                persons.Add(person);
            }
        }

        return new LoanWithPersons()
        {
            LoanDto = loan,
            Persons = persons
        };
    }

    public async Task<IEnumerable<LoanWithPersons>> GetLoansByPersonIdAsync(Guid personId)
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();

        var loanIds = await dbConnection.QueryAsync<Guid>(
            "SELECT DISTINCT(LoanId) FROM PersonToLoan WHERE PersonId = @PersonId",
            new { PersonId = personId }
        );
        var loans = new List<LoanWithPersons>();
        foreach (var loanId in loanIds)
        {
            var loan = await dbConnection.QueryFirstOrDefaultAsync<LoanDto>(
                "SELECT * FROM Loan WHERE Id = @Id",
                new { Id = loanId }
            );
            if (loan != null)
            {
                loans.Add(
                    new LoanWithPersons()
                    {
                        LoanDto = loan,
                        Persons = null
                    });
            }
        }

        return loans;
    }

    public async Task<LoanDto> CreateLoanAsync(LoanDto loanDto, IEnumerable<Guid> personIds)
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            // Insert the loan into the database
            await dbConnection.ExecuteAsync(
                "INSERT INTO Loan (LoanBaseAmount, Purpose, DurationInDays, StartDatetimeUtc, Interest, LoanTotalAmount, IsApproved) " +
                "VALUES (@LoanBaseAmount, @Purpose, @DurationInDays, @StartDatetimeUtc, @Interest, @LoanTotalAmount, @IsApproved)",
                loanDto,
                transaction
            );

            // Retrieve the newly created loan from the database
            var createdLoan = await dbConnection.QueryFirstOrDefaultAsync<LoanDto>(
                "SELECT TOP 1 * FROM Loan ORDER BY StartDatetimeUtc DESC",
                transaction: transaction
            );

            // Insert the person-to-loan relationships into the database
            foreach (var personId in personIds)
            {
                await dbConnection.ExecuteAsync(
                    "INSERT INTO PersonToLoan (PersonId, LoanId) " +
                    "VALUES (@PersonId, @LoanId)",
                    new { PersonId = personId, LoanId = createdLoan.Id },
                    transaction
                );
            }

            transaction.Commit();

            // Return the newly created loan
            return createdLoan;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Boolean> SetLoanToApprovedAsync(Guid loanId)
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        await dbConnection.ExecuteAsync(
            "UPDATE Loan SET IsApproved = 1 " +
            "WHERE Id = @Id",
            new { Id = loanId }
        );
        return true;
    }
}