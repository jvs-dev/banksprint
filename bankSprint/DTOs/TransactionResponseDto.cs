using BankSprint.Models;

namespace BankSprint.DTOs;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}
