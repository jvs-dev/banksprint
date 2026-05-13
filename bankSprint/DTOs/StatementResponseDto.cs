namespace BankSprint.DTOs;

public class StatementResponseDto
{
    public AccountResponseDto Account { get; set; } = null!;
    public IReadOnlyList<TransactionResponseDto> Transactions { get; set; } = Array.Empty<TransactionResponseDto>();
}
