using BankSprint.DTOs;
using BankSprint.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankSprint.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IWebHostEnvironment env)
    {
        _authService = authService;
        _logger = logger;
        _env = env;
    }

    /// <summary>Cadastro de nova conta (sem JWT na resposta).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _authService.RegisterAsync(dto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro de banco no cadastro.");
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) ||
                inner.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { erro = "Este e-mail ja esta cadastrado." });

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                erro = "Erro ao salvar no banco de dados. Verifique a connection string, se o MySQL esta ativo e se as tabelas existem.",
                detalhe = _env.IsDevelopment() ? inner : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado no cadastro.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                erro = "Erro interno ao processar o cadastro.",
                detalhe = _env.IsDevelopment() ? ex.Message : null
            });
        }
    }

    /// <summary>Login retornando token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        if (result is null)
            return Unauthorized(new { erro = "E-mail ou senha invalidos." });

        return Ok(result);
    }
}
