using BankSprint.DTOs;
using BankSprint.Repositories;

namespace BankSprint.Services;

public class AccountService : IAccountService
{
    private const string AdminRole = "Administrador";

    private readonly IAccountRepository _accounts;

    public AccountService(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<IReadOnlyList<AccountResponseDto>> ListAsync(int callerId, string callerRole, CancellationToken cancellationToken = default)
    {
        if (string.Equals(callerRole, AdminRole, StringComparison.OrdinalIgnoreCase))
        {
            var all = await _accounts.GetAllAsync(cancellationToken);
            return all.Select(AccountMapper.ToDto).ToList();
        }

        var self = await _accounts.GetByIdAsync(callerId, cancellationToken);
        if (self is null)
            return Array.Empty<AccountResponseDto>();

        return new[] { AccountMapper.ToDto(self) };
    }

    public async Task<AccountResponseDto> UpdateMeAsync(int callerId, AccountUpdateRequestDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdTrackedAsync(callerId, cancellationToken);
        if (account is null)
            throw new KeyNotFoundException("Conta não encontrada.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var other = await _accounts.GetByEmailAsync(email, cancellationToken);
        if (other is not null && other.Id != callerId)
            throw new InvalidOperationException("Este e-mail já está em uso por outra conta.");

        account.Name = dto.Name.Trim();
        account.Email = email;
        await _accounts.UpdateAsync(account, cancellationToken);
        return AccountMapper.ToDto(account);
    }

    public async Task DeleteMeAsync(int callerId, CancellationToken cancellationToken = default)
    {
        var account = await _accounts.GetByIdTrackedAsync(callerId, cancellationToken);
        if (account is null)
            throw new KeyNotFoundException("Conta não encontrada.");

        await _accounts.DeleteAsync(account, cancellationToken);
    }
}
