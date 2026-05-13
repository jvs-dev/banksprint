using BankSprint.Data;
using BankSprint.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSprint.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Transaction>> ListByAccountIdAsync(int accountId, CancellationToken cancellationToken = default) =>
        await _context.Transactions.AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
}
