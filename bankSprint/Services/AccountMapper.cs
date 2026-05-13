using BankSprint.DTOs;
using BankSprint.Models;

namespace BankSprint.Services;

public static class AccountMapper
{
    public static AccountResponseDto ToDto(Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Email = account.Email,
        Balance = account.Balance,
        Role = account.Role
    };

    public static TransactionResponseDto ToDto(Transaction transaction) => new()
    {
        Id = transaction.Id,
        Type = transaction.Type,
        Amount = transaction.Amount,
        Date = transaction.Date
    };
}
