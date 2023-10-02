using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using denicestbankportal.Models;

namespace denicestbankportal.Database;

public class TransactionProvider
{
    private readonly String _connectionString_;

    public TransactionProvider(string connectionString)
    {
        _connectionString_ = connectionString;
    }

    public async Task<IEnumerable<LoanLatestState>> GetAllLoansLatestStates()
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        return await dbConnection.QueryAsync<LoanLatestState>($@"
            SELECT LoanId, SUM(Amount) AS TotalTransacted
            FROM Transact
            GROUP BY LoanId;");
    }
    public async Task<LoanLatestState> GetLoanLatestState(Guid loanId)
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        return await dbConnection.QueryFirstOrDefaultAsync<LoanLatestState>($@"
            SELECT LoanId, SUM(Amount) AS TotalTransacted
            FROM Transact
            Where LoanId = @loanId;", new { loanId = loanId });
    }

    public async Task<Transact> InsertTransactionAsync(Transact transaction)
    {
        using IDbConnection dbConnection = new SqlConnection(_connectionString_);
        dbConnection.Open();
        transaction.UpdateDatetimeUtc = DateTime.UtcNow;
        var query =
            "INSERT INTO Transact (PersonId, LoanId, UpdateDatetimeUtc, Amount) " +
            "OUTPUT INSERTED.Id " +
            "VALUES (@PersonId, @LoanId, @UpdateDatetimeUtc, @Amount)";
        var id = await dbConnection.ExecuteScalarAsync<Guid>(query, transaction);
        transaction.Id = id;
        return transaction;
    }
}