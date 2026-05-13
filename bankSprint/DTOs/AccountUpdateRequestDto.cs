using System.ComponentModel.DataAnnotations;

namespace BankSprint.DTOs;

public class AccountUpdateRequestDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
