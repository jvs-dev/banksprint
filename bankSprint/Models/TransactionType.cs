namespace BankSprint.Models;

public enum TransactionType
{
    Deposit = 0,
    Withdraw = 1,
    /// <summary>Transferência entre contas: valor negativo = saída, positivo = entrada.</summary>
    Transfer = 2
}
