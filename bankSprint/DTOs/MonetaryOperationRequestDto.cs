using System.ComponentModel.DataAnnotations;

namespace BankSprint.DTOs;

public class MonetaryOperationRequestDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }
}
