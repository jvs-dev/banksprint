using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BankSprint.DTOs;
using BankSprint.Models;
using BankSprint.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BankSprint.Services;

public class AuthService : IAuthService
{
    private const string DefaultRole = "Cliente";

    private readonly IAccountRepository _accounts;
    private readonly IPasswordHasher<Account> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IAccountRepository accounts,
        IPasswordHasher<Account> passwordHasher,
        IConfiguration configuration)
    {
        _accounts = accounts;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AccountResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        var name = (dto.Name ?? string.Empty).Trim();
        var existing = await _accounts.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Este e-mail já está cadastrado.");

        var account = new Account
        {
            Name = name,
            Email = email,
            Balance = 0,
            Role = DefaultRole,
            PasswordHash = string.Empty
        };

        account.PasswordHash = _passwordHasher.HashPassword(account, dto.Password);
        await _accounts.AddAsync(account, cancellationToken);
        return AccountMapper.ToDto(account);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var account = await _accounts.GetByEmailTrackedAsync(email, cancellationToken);
        if (account is null)
            return null;

        var verify = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            return null;

        var expires = DateTime.UtcNow.AddHours(2);
        var token = CreateJwtToken(account, expires);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expires,
            Message = "Login realizado com sucesso."
        };
    }

    private string CreateJwtToken(Account account, DateTime expiresUtc)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(ClaimTypes.Name, account.Name),
            new(ClaimTypes.Role, account.Role)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresUtc,
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
