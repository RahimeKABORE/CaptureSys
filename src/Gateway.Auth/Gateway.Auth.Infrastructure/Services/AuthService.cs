using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CaptureSys.Gateway.Auth.Application.Interfaces;
using CaptureSys.Gateway.Auth.Domain.Entities;
using CaptureSys.Shared.Results;
using CaptureSys.Shared.DTOs;
using CaptureSys.Gateway.Auth.Domain;

namespace CaptureSys.Gateway.Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, User> _users;

    public AuthService(ILogger<AuthService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _users = new Dictionary<string, User>();
        InitializeDefaultUsers();
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        await Task.Yield();

        _logger.LogInformation("Tentative de connexion pour {Username}", request.Username);

        if (!_users.TryGetValue(request.Username, out var user))
        {
            _logger.LogWarning("Utilisateur {Username} non trouvé", request.Username);
            return Result<AuthResponse>.Failure("Utilisateur non trouvé");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Compte {Username} désactivé", request.Username);
            return Result<AuthResponse>.Failure("Compte désactivé");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Mot de passe incorrect pour {Username}", request.Username);
            return Result<AuthResponse>.Failure("Mot de passe incorrect");
        }

        var authResponse = new AuthResponse
        {
            Token = "simple-token-" + Guid.NewGuid().ToString("N")[..8],
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = MapToUserInfo(user)
        };

        user.UpdateLastLogin();
        _logger.LogInformation("Connexion réussie pour {Username}", request.Username);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        await Task.Yield();

        _logger.LogInformation("Tentative d'inscription pour {Username}", request.Username);

        if (_users.ContainsKey(request.Username))
        {
            return Result<AuthResponse>.Failure("Nom d'utilisateur déjà existant");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Username, request.Email, passwordHash, request.FirstName, request.LastName);
        _users[request.Username] = user;

        var authResponse = new AuthResponse
        {
            Token = "fake-jwt-token-for-now",
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = MapToUserInfo(user)
        };

        _logger.LogInformation("Inscription réussie pour {Username}", request.Username);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<TokenValidationResult>> ValidateTokenAsync(string token)
    {
        await Task.Yield();

        var result = new TokenValidationResult
        {
            IsValid = !string.IsNullOrEmpty(token),
            Username = "test-user",
            Roles = new List<string> { "Admin" }
        };

        return Result<TokenValidationResult>.Success(result);
    }

    public async Task<Result<UserInfo>> GetUserInfoAsync(string username)
    {
        await Task.Yield();

        if (_users.TryGetValue(username, out var user))
        {
            return Result<UserInfo>.Success(MapToUserInfo(user));
        }

        return Result<UserInfo>.Failure("Utilisateur non trouvé");
    }

    public async Task<Result<bool>> LogoutAsync(string username)
    {
        await Task.Yield();
        _logger.LogInformation("Déconnexion pour {Username}", username);
        return Result<bool>.Success(true);
    }

    public async Task<Result<List<UserInfo>>> GetUsersAsync()
    {
        await Task.Yield();

        try
        {
            var users = _users.Values.Select(MapToUserInfo).ToList();
            _logger.LogInformation("Récupération de {Count} utilisateurs", users.Count);
            return Result<List<UserInfo>>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs");
            return Result<List<UserInfo>>.Failure($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUserRolesAsync(string username, List<UserRole> roles)
    {
        await Task.Yield();

        try
        {
            if (!_users.TryGetValue(username, out var user))
            {
                return Result<bool>.Failure("Utilisateur non trouvé");
            }

            var roleStrings = roles?.Select(r => r.ToString()).ToList() ?? new List<string>();
            user.UpdateRoles(roleStrings);
            
            _logger.LogInformation("Rôles mis à jour pour {Username}: {Roles}", username, string.Join(", ", roleStrings));
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des rôles pour {Username}", username);
            return Result<bool>.Failure($"Erreur: {ex.Message}");
        }
    }

    private static UserInfo MapToUserInfo(User user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.Roles,
            IsActive = user.IsActive
        };
    }

    private void InitializeDefaultUsers()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        var adminUser = new User("admin", "admin@capturesys.com", passwordHash, "Admin", "System");
        adminUser.UpdateRoles(new List<string> { "Admin" });
        _users["admin"] = adminUser;
        _logger.LogInformation("Utilisateur admin par défaut créé");
    }
}