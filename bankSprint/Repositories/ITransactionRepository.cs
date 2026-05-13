using BankSprint.Models;

namespace BankSprint.Repositories;

public interface ITransactionRepository
{
    Task<IReadOnlyList<Transaction>> ListByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
}
