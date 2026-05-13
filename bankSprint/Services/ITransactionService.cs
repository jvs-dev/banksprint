using BankSprint.DTOs;

namespace BankSprint.Services;

public interface ITransactionService
{
    Task<AccountResponseDto> DepositAsync(int accountId, MonetaryOperationRequestDto dto, CancellationToken cancellationToken = default);
    Task<AccountResponseDto> WithdrawAsync(int accountId, MonetaryOperationRequestDto dto, CancellationToken cancellationToken = default);
    Task<AccountResponseDto> TransferAsync(int fromAccountId, TransferRequestDto dto, CancellationToken cancellationToken = default);
    Task<StatementResponseDto> GetStatementAsync(int accountId, CancellationToken cancellationToken = default);
}
