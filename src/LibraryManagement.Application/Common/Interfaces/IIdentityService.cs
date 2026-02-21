using LibraryManagement.Application.Common.Models;

namespace LibraryManagement.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> AssignRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
