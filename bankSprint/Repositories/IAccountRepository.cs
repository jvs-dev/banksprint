using BankSprint.Models;

namespace BankSprint.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Account?> GetByEmailTrackedAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task DeleteAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>Atualiza saldo e grava transação de depósito em uma única operação.</summary>
    Task<Account> ExecuteDepositAsync(int accountId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>Atualiza saldo e grava transação de saque em uma única operação.</summary>
    Task<Account> ExecuteWithdrawAsync(int accountId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>Transfere entre duas contas (duas linhas tipo Transfer: valor negativo na origem, positivo no destino).</summary>
    Task<Account> ExecuteTransferAsync(int fromAccountId, int toAccountId, decimal amount, CancellationToken cancellationToken = default);
}
