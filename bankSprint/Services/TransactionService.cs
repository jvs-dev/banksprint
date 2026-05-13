using BankSprint.DTOs;
using BankSprint.Repositories;

namespace BankSprint.Services;

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;

    public TransactionService(IAccountRepository accounts, ITransactionRepository transactions)
    {
        _accounts = accounts;
        _transactions = transactions;
    }

    public async Task<AccountResponseDto> DepositAsync(int accountId, MonetaryOperationRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");

        var exists = await _accounts.GetByIdAsync(accountId, cancellationToken);
        if (exists is null)
            throw new KeyNotFoundException("Conta não encontrada.");

        var updated = await _accounts.ExecuteDepositAsync(accountId, dto.Amount, cancellationToken);
        return AccountMapper.ToDto(updated);
    }

    public async Task<AccountResponseDto> WithdrawAsync(int accountId, MonetaryOperationRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");

        var exists = await _accounts.GetByIdAsync(accountId, cancellationToken);
        if (exists is null)
            throw new KeyNotFoundException("Conta não encontrada.");

        var updated = await _accounts.ExecuteWithdrawAsync(accountId, dto.Amount, cancellationToken);
        return AccountMapper.ToDto(updated);
    }

    public async Task<AccountResponseDto> TransferAsync(int fromAccountId, TransferRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("O valor deve ser maior que zero.");

        if (dto.ToAccountId <= 0)
            throw new ArgumentException("Informe o ID da conta de destino.");

        var exists = await _accounts.GetByIdAsync(fromAccountId, cancellationToken);
        if (exists is null)
            throw new KeyNotFoundException("Conta não encontrada.");

        var updated = await _accounts.ExecuteTransferAsync(fromAccountId, dto.ToAccountId, dto.Amount, cancellationToken);
        return AccountMapper.ToDto(updated);
    }

    public async Task<StatementResponseDto> GetStatementAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
            throw new KeyNotFoundException("Conta não encontrada.");

        var list = await _transactions.ListByAccountIdAsync(accountId, cancellationToken);
        return new StatementResponseDto
        {
            Account = AccountMapper.ToDto(account),
            Transactions = list.Select(AccountMapper.ToDto).ToList()
        };
    }
}
