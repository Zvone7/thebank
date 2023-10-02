using System.Transactions;
using denicestbankportal.Logic;
using denicestbankportal.Models;
    using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace denicestbankportal.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<ActionResult<Transact>> PerformTransaction(Transact transaction)
        {
            var createdTransaction = await _transactionService.InsertTransactionAsync(transaction);
            return CreatedAtAction(nameof(PerformTransaction), new { id = transaction.Id }, createdTransaction);
        }

    }
}