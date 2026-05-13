using BankSprint.Data;
using BankSprint.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankSprint.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Account?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email, cancellationToken);

    public async Task<Account?> GetByEmailTrackedAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Accounts.AsNoTracking().OrderBy(a => a.Id).ToListAsync(cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Account> ExecuteDepositAsync(int accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FirstAsync(a => a.Id == accountId, cancellationToken);
        account.Balance += amount;
        await _context.Transactions.AddAsync(new Transaction
        {
            AccountId = accountId,
            Type = TransactionType.Deposit,
            Amount = amount,
            Date = DateTime.UtcNow
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account> ExecuteWithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts.FirstAsync(a => a.Id == accountId, cancellationToken);
        if (account.Balance < amount)
            throw new InvalidOperationException("Saldo insuficiente para realizar o saque.");

        account.Balance -= amount;
        await _context.Transactions.AddAsync(new Transaction
        {
            AccountId = accountId,
            Type = TransactionType.Withdraw,
            Amount = amount,
            Date = DateTime.UtcNow
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account> ExecuteTransferAsync(int fromAccountId, int toAccountId, decimal amount, CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var from = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == fromAccountId, cancellationToken);
        if (from is null)
            throw new KeyNotFoundException("Conta de origem não encontrada.");

        var to = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == toAccountId, cancellationToken);
        if (to is null)
            throw new KeyNotFoundException("Conta de destino não encontrada.");

        if (from.Id == to.Id)
            throw new InvalidOperationException("Não é possível transferir para a própria conta.");

        if (from.Balance < amount)
            throw new InvalidOperationException("Saldo insuficiente para realizar a transferência.");

        var utc = DateTime.UtcNow;
        from.Balance -= amount;
        to.Balance += amount;

        await _context.Transactions.AddAsync(new Transaction
        {
            AccountId = from.Id,
            Type = TransactionType.Transfer,
            Amount = -amount,
            Date = utc
        }, cancellationToken);

        await _context.Transactions.AddAsync(new Transaction
        {
            AccountId = to.Id,
            Type = TransactionType.Transfer,
            Amount = amount,
            Date = utc
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return from;
    }
}
