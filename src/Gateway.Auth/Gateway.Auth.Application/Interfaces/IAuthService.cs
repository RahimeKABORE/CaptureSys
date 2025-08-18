using CaptureSys.Gateway.Auth.Domain.Entities;
using CaptureSys.Shared.Results;
using CaptureSys.Shared.DTOs;

namespace CaptureSys.Gateway.Auth.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<TokenValidationResult>> ValidateTokenAsync(string token);
    Task<Result<UserInfo>> GetUserInfoAsync(string username);
    Task<Result<bool>> LogoutAsync(string username);
    Task<Result<List<UserInfo>>> GetUsersAsync();
    Task<Result<bool>> UpdateUserRolesAsync(string username, List<UserRole> roles);
}