namespace BankSprint.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Role { get; set; } = "Cliente";

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
