using BankSprint.DTOs;

namespace BankSprint.Services;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponseDto>> ListAsync(int callerId, string callerRole, CancellationToken cancellationToken = default);
    Task<AccountResponseDto> UpdateMeAsync(int callerId, AccountUpdateRequestDto dto, CancellationToken cancellationToken = default);
    Task DeleteMeAsync(int callerId, CancellationToken cancellationToken = default);
}
