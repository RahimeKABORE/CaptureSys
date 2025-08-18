using CaptureSys.Shared.Entities;

namespace CaptureSys.Gateway.Auth.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public List<string> Roles { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public User(string username, string email, string passwordHash, string firstName, string lastName)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Roles = new List<string> { "Viewer" };
        IsActive = true;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdateRefreshToken(string? refreshToken, DateTime? expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void UpdateRoles(List<string> roles)
    {
        Roles = roles ?? new List<string>();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Operator = 3,
    Viewer = 4
}

public enum UserStatus
{
    Pending = 1,
    Active = 2,
    Inactive = 3,
    Locked = 4
}
