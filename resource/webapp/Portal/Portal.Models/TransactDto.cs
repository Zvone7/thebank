namespace Portal.Models;

public class TransactDto
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid LoanId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDatetimeUtc { get; set; }
    public Decimal Amount { get; set; }
}