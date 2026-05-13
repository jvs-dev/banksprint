using BankSprint.DTOs;

namespace BankSprint.Services;

public interface IAuthService
{
    Task<AccountResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}
