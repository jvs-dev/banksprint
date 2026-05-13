namespace BankSprint.Models;

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public Account? Account { get; set; }
}
