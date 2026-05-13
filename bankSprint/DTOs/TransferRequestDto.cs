using System.ComponentModel.DataAnnotations;

namespace BankSprint.DTOs;

public class TransferRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Informe um ID de conta de destino válido.")]
    public int ToAccountId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }
}
