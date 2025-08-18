using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaptureSys.Gateway.Auth.Application.Interfaces;
using CaptureSys.Shared.DTOs;

namespace CaptureSys.Gateway.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    /// <summary>
    /// Test endpoint pour vérifier que le service fonctionne
    /// </summary>
    [HttpGet]
    public ActionResult GetStatus()
    {
        return Ok(new
        {
            Service = "Gateway.Auth",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Port = 5006,
            Endpoints = new[]
            {
                "GET /api/Auth - Status",
                "POST /api/Auth/login - Login user",
                "POST /api/Auth/register - Register user",
                "POST /api/Auth/validate - Validate token",
                "GET /api/Auth/me - Get current user",
                "POST /api/Auth/logout - Logout user"
            }
        });
    }

    /// <summary>
    /// Connexion utilisateur
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Tentative de connexion pour {Username}", request.Username);

            var result = await _authService.LoginAsync(request);

            if (result.IsFailure)
            {
                _logger.LogWarning("Échec de connexion pour {Username}: {Error}", request.Username, result.Error);
                return Unauthorized(result.Error);
            }

            _logger.LogInformation("Connexion réussie pour {Username}", request.Username);
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion pour {Username}", request.Username);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Inscription utilisateur
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Tentative d'inscription pour {Username}", request.Username);

            var result = await _authService.RegisterAsync(request);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            _logger.LogInformation("Inscription réussie pour {Username}", request.Username);
            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription pour {Username}", request.Username);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Validation de token
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<TokenValidationResult>> ValidateToken([FromBody] string token)
    {
        try
        {
            var result = await _authService.ValidateTokenAsync(token);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation de token");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Obtenir les informations de l'utilisateur connecté
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Utilisateur non identifié");
            }

            var result = await _authService.GetUserInfoAsync(username);

            if (result.IsFailure)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des informations utilisateur");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Déconnexion
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Utilisateur non identifié");
            }

            var result = await _authService.LogoutAsync(username);

            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            _logger.LogInformation("Déconnexion réussie pour {Username}", username);
            return Ok(new { Message = "Déconnexion réussie" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
}