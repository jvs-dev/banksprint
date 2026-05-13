namespace BankSprint.DTOs;

public class AccountResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Role { get; set; } = string.Empty;
}
